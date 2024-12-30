using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

namespace ChatServer
{
    /*
     * 데이터 처리방식
     * Streaming : 실시간으로 끊어짐없이 데이터를 전달하는 방식
     * Messaging Queue : 대기열에 쌓아두고 필요할때마다 전달하는 방식
     */
    public abstract class TcpSession
    {
        public TcpSession()
        {
            SendQueue = new Queue<ArraySegment<byte>>(20);
            BufferPool = ArrayPool<byte>.Shared;
            SendStream = new MemoryStream(64 * KB);
            SendStreamWriter = new BinaryReader(SendStream);
            ReceiveStream = new MemoryStream(64 * KB);
            ReceiveStreamReader = new BinaryReader(ReceiveStream);
        }


        public bool IsConnected { get; protected set; }


        protected Socket Socket;
        protected Task ReceiveTask;
        protected const int KB = 1_024;
        protected readonly Queue<ArraySegment<byte>> SendQueue;
        protected readonly ArrayPool<byte> BufferPool;
        protected MemoryStream SendStream;
        protected BinaryReader SendStreamWriter;
        protected MemoryStream ReceiveStream;
        protected BinaryReader ReceiveStreamReader;

        public event Action OnConnected;
        public event Action OnDisconnected;


        public void Connect(IPEndPoint remoteEndPoint)
        {
            Socket.Connect(remoteEndPoint);
            IsConnected = Socket.Connected;

            if (IsConnected)
            {
                OnConnected?.Invoke();
                ReceiveTask = Task.Run(ReceiveLoopAsync);
            }
            else
            {
                Console.WriteLine($"[TCP Session] Failed to connect to {remoteEndPoint}.");
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            IsConnected = false;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            Socket.Close();
            Socket.Dispose();
            OnDisconnected?.Invoke();
            Console.WriteLine($"[TCP Session] Disconnected.");
        }

        public void Send(IPayload payload)
        {
            if (!IsConnected)
            {
                Console.WriteLine($"[TCP Session] Failed to send data. Disconnected.");
                return;
            }

            int estimatedSize = 1 * KB;
            byte[] buffer = BufferPool.Rent(estimatedSize);

            // 2의 보수 <- 2진법에서 모든자릿수를 반전시키고 + 1 
            // 7 = 0111 
            // -7 = 1001
            // -(-7) = 0111 = 7
            // 0
                        
            // (+1) + (-1) = 0

            try
            {
                byte[] entireData = Serialize(payload);
                int offset = 0;
                int entireLength = entireData.Length;

                while (offset < entireLength)
                {
                    int chunkSize = Math.Min(1 * KB, entireLength - offset);
                    byte[] chunk = BufferPool.Rent(chunkSize);
                    Buffer.BlockCopy(entireData, offset, chunk, 0, chunkSize);

                    ArraySegment<byte> segment = new ArraySegment<byte>(chunk);
                    SendQueue.Enqueue(segment);
                    offset += chunkSize;
                }

                _ = SendLoopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            
        }

        byte[] Serialize(IPayload payload)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)payload.PayloadType); // 패킷 타입 쓰기
                writer.Write(0); // Payload 길이. 일단 임의의 정수를 할당하고, 이후에 계산해서 실제 길이 넣을거임.
                long payloadStartPosition = stream.Position; // 실제 데이터 시작위치
                payload.Serialize(writer);
                long payloadEndPosition = stream.Position; // 실제 데이터 끝위치
                long payloadLength = payloadEndPosition - payloadStartPosition; // 실제데이터 길이
                stream.Position = sizeof(ushort); // 패킷타입 뒤로 이동
                writer.Write(payloadLength); // Payload 길이 덮어씀.

                return stream.ToArray();
            }
        }

        async Task SendLoopAsync()
        {
            while (SendQueue.TryDequeue(out ArraySegment<byte> segment))
            {
                try
                {
                    int bytesSent = await Socket.SendAsync(segment);
                    Console.WriteLine($"[TCP Session] : Sent data length of {bytesSent}.");

                    BufferPool.Return(segment.Array);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TCP Session] : Failed to send data.");
                    Disconnect();
                    break;
                }
            }
        }

        async Task ReceiveLoopAsync()
        {
            byte[] buffer = BufferPool.Rent(1 * KB);

            while (IsConnected)
            {
                try
                {
                    int bytesRead = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer));

                    // 유효한 데이터 안들어왔으면 연결 끊어진것으로 간주
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"[TCP Session] Remote host closed the connection.");
                        Disconnect();
                        break;
                    }

                    ReceiveStream.Seek(0, SeekOrigin.End); // Segment 누적을위해서 가장 끝으로이동
                    ReceiveStream.Write(buffer, 0, bytesRead);
                    ReceiveStream.Seek(0, SeekOrigin.Begin); // 전체 Payload 파싱이 가능한지 확인하기위해서 가장 앞으로이동

                    while (true)
                    {
                        if (ReceiveStream.Length - ReceiveStream.Position < sizeof(PayloadType) + sizeof(int))
                            break;

                        PayloadType payloadType = (PayloadType)ReceiveStreamReader.ReadUInt16();
                        int payloadLength = ReceiveStreamReader.ReadInt32();

                        // 아직 Payload 전체가 도착하지 않았다면 다음 세그먼트 수신 대기하러가야함
                        if (ReceiveStream.Length - ReceiveStream.Position < payloadLength)
                        {
                            ReceiveStream.Seek(0, SeekOrigin.Begin);
                            break;
                        }

                        IPayload payload = PayloadFactory.Create(payloadType);

                        if (payload == null)
                        {
                            Console.WriteLine($"[{nameof(TcpSession)}] Invalid payloadType {payloadType}.");
                            break;
                        }

                        payload.Deserialize(ReceiveStreamReader);
                        HandlePayload(payload);
                    }

                    string receivedData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[TCP Session] received from {Socket.RemoteEndPoint}. {receivedData} total {bytesRead} bytes.");
                }
                catch (ObjectDisposedException ex)
                {
                    // 세션이 만료되었는데 비동기 수신 task 가 정상적으로 완료되지 못함 예외
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 수신받은 데이터를 처리
        /// </summary>
        /// <param name="payload"> 수신받은 데이터 </param>
        protected abstract void HandlePayload(IPayload payload);
    }
}
