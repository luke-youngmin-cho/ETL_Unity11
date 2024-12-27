using System.Buffers;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        }


        public bool IsConnected { get; protected set; }


        protected Socket Socket;
        protected Task ReceiveTask;
        protected const int KB = 1_024;
        protected readonly Queue<ArraySegment<byte>> SendQueue;
        protected readonly ArrayPool<byte> BufferPool;

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

        public void Send(IPayload packet)
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
                using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, true))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((ushort)packet.PayloadType); // 패킷 타입 쓰기
                    writer.Write(0); // Payload 길이. 일단 임의의 정수를 할당하고, 이후에 계산해서 실제 길이 넣을거임.
                    long payloadStartPosition = stream.Position; // 실제 데이터 시작위치
                    packet.Serialize(writer);
                    long payloadEndPosition = stream.Position; // 실제 데이터 끝위치
                    long payloadLength = payloadEndPosition - payloadStartPosition; // 실제데이터 길이
                    stream.Position = sizeof(ushort); // 패킷타입 뒤로 이동
                    writer.Write(payloadLength); // Payload 길이 덮어씀.
                    stream.Position = payloadEndPosition;
                    int bytesWritten = (int)stream.Position; // 총 Segment 길이
                    ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, bytesWritten);
                    SendQueue.Enqueue(segment);
                    Console.WriteLine($"[TCP Session] send to {Socket.RemoteEndPoint}. total {bytesWritten} bytes.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            _ = SendLoopAsync();
        }

        async Task SendLoopAsync()
        {
            while (SendQueue.TryDequeue(out ArraySegment<byte> segment))
            {
                try
                {
                    int bytesSent = await Socket.SendAsync(segment);
                    Console.WriteLine($"[TCP Session] : Sent data length of {bytesSent}.");
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
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine($"[TCP Session] Remote host closed the connection.");
                        Disconnect();
                        break;
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
    }
}
