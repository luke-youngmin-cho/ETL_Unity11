using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DebugSystems;

namespace Practices.GameClient.Network
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
            ReceiveStream = new MemoryStream(64 * KB);
            ReceiveStreamReader = new BinaryReader(ReceiveStream);
        }


        public bool IsConnected { get; protected set; }


        protected Socket Socket;
        protected Task ReceiveTask;
        protected const int KB = 1_024;
        protected readonly Queue<ArraySegment<byte>> SendQueue;
        protected readonly ArrayPool<byte> BufferPool;
        protected MemoryStream ReceiveStream;
        protected BinaryReader ReceiveStreamReader;
        protected readonly object SendLock = new object();

        public event Action OnConnected;
        public event Action OnDisconnected;


        public void Connect(EndPoint remoteEndPoint)
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
                DebugLogger.Log($"[TCP Session] Failed to connect to {remoteEndPoint}.");
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
            DebugLogger.Log($"[TCP Session] Disconnected.");

        }


        public void Send(IMessage message)
        {
            if (!IsConnected)
            {
                DebugLogger.Log($"[TCP Session] Failed to send data. Disconnected.");
                return;
            }

            // 2의 보수 <- 2진법에서 모든자릿수를 반전시키고 + 1 
            // 7 = 0111 
            // -7 = 1001
            // -(-7) = 0111 = 7
            // 0

            // (+1) + (-1) = 0

            try
            {
                byte[] entireData = Serialize(message);
                int offset = 0;
                int entireLength = entireData.Length;

                lock (SendLock)
                {
                    while (offset < entireLength)
                    {
                        int chunkSize = Math.Min(1 * KB, entireLength - offset);
                        byte[] chunk = BufferPool.Rent(chunkSize);
                        Buffer.BlockCopy(entireData, offset, chunk, 0, chunkSize);

                        ArraySegment<byte> segment = new ArraySegment<byte>(chunk, 0, chunkSize);
                        SendQueue.Enqueue(segment);
                        offset += chunkSize;
                    }
                }

                _ = SendLoopAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }

        byte[] Serialize(IMessage message)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)message.MessageType); // 패킷 타입 쓰기
                writer.Write(0); // message 길이. 일단 임의의 정수를 할당하고, 이후에 계산해서 실제 길이 넣을거임.
                int messageStartPosition = (int)stream.Position; // 실제 데이터 시작위치
                message.Serialize(writer);
                int messageEndPosition = (int)stream.Position; // 실제 데이터 끝위치
                int messageLength = messageEndPosition - messageStartPosition; // 실제데이터 길이
                stream.Position = sizeof(ushort); // 패킷타입 뒤로 이동
                writer.Write(messageLength); // message 길이 덮어씀.

                return stream.ToArray();
            }
        }

        async Task SendLoopAsync()
        {
            while (SendQueue.TryDequeue(out ArraySegment<byte> segment))
            {
                try
                {
                    int offset = 0;

                    // Socket.SendAsync 가 segment 전체를 한번에 보낸다는 보장이 없기때문에, 다 보낼때까지 순회하면서 segment 전체를 보냄.
                    while (offset < segment.Count)
                    {
                        int remain = segment.Count - offset;
                        int bytesSent = await Socket.SendAsync(new ArraySegment<byte>(segment.Array, segment.Offset + offset, remain), SocketFlags.None);

                        if (bytesSent <= 0)
                        {
                            DebugLogger.Log($"[TCP Session] : Something went wrong.");
                            Disconnect();
                            return;
                        }

                        offset += bytesSent;
                        DebugLogger.Log($"[TCP Session] : Sent data length of {bytesSent}.");
                    }

                    BufferPool.Return(segment.Array);
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[TCP Session] : Failed to send data.");
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
                    int bytesRead = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    // 유효한 데이터 안들어왔으면 연결 끊어진것으로 간주
                    if (bytesRead == 0)
                    {
                        DebugLogger.Log($"[TCP Session] : Remote host closed the connection.");
                        Disconnect();
                        break;
                    }

                    ReceiveStream.Seek(0, SeekOrigin.End); // Segment 누적을위해서 가장 끝으로이동
                    ReceiveStream.Write(buffer, 0, bytesRead);
                    ReceiveStream.Seek(0, SeekOrigin.Begin); // 전체 message 파싱이 가능한지 확인하기위해서 가장 앞으로이동

                    while (true)
                    {
                        if (ReceiveStream.Length - ReceiveStream.Position < sizeof(MessageType) + sizeof(int))
                            break;

                        MessageType messageType = (MessageType)ReceiveStreamReader.ReadUInt16();
                        int messageLength = ReceiveStreamReader.ReadInt32();

                        // 아직 message 전체가 도착하지 않았다면 다음 세그먼트 수신 대기하러가야함
                        if (ReceiveStream.Length - ReceiveStream.Position < messageLength)
                        {
                            ReceiveStream.Seek(0, SeekOrigin.Begin);
                            break;
                        }

                        IMessage message = MessageFactory.Create(messageType);

                        if (message == null)
                        {
                            DebugLogger.Log($"[{nameof(TcpSession)}] Invalid messageType {messageType}.");
                            break;
                        }

                        message.Deserialize(ReceiveStreamReader);
                        HandleMessage(message);
                        Console.WriteLine($"[TCP Session] received from {Socket.RemoteEndPoint}. {message.MessageType}.");

                    }

                    // 파싱 후 남은 세그먼트데이터를 버퍼 맨앞으로 밀착
                    long remain = ReceiveStream.Length - ReceiveStream.Position;

                    if (remain > 0)
                    {
                        Buffer.BlockCopy(ReceiveStream.GetBuffer(), (int)ReceiveStream.Position, ReceiveStream.GetBuffer(), 0, (int)remain);
                    }

                    ReceiveStream.Position = remain;
                    ReceiveStream.SetLength(remain);
                }
                catch (ObjectDisposedException ex)
                {
                    // 세션이 만료되었는데 비동기 수신 task 가 정상적으로 완료되지 못함 예외
                    DebugLogger.LogError(ex.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogError(ex.ToString());
                    throw new Exception(ex.ToString());
                }

                await Task.Delay(100);
            }

            BufferPool.Return(buffer);
        }

        /// <summary>
        /// 수신받은 데이터를 처리
        /// </summary>
        /// <param name="message"> 수신받은 데이터 </param>
        protected abstract void HandleMessage(IMessage message);
    }
}
