using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    /*
     * Session : 소통에 필요한 정보들을 유지하고있는 기간\
     * UDP (User Datagram Protocol) : 데이터 날리고 끝. 검증 없음
     */
    public abstract class UdpSession
    {
        public UdpSession()
        {
            SendQueue = new Queue<(ArraySegment<byte> segment, EndPoint remoteEndPoint)>(200);
            BufferPool = ArrayPool<byte>.Shared;
        }


        public bool IsRunning { get; protected set; }

        protected Socket Socket;
        protected Task SendTask;
        protected Task ReceiveTask;
        protected const int KB = 1_024;
        protected Queue<(ArraySegment<byte> segment, EndPoint remoteEndPoint)> SendQueue;
        protected readonly ArrayPool<byte> BufferPool;


        public void Start()
        {
            IsRunning = true;
            SendTask = Task.Run(SendLoopAsync);
            ReceiveTask = Task.Run(ReceiveLoopAsync);
        }

        public void Stop()
        {
            IsRunning = false;
            Socket.Close();
            Socket.Dispose();
        }

        void Send(IPayload packet, EndPoint remoteEndPoint)
        {
            int estimatedSize = 1 * KB;
            byte[] buffer = BufferPool.Rent(estimatedSize);

            try
            {
                using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, true))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((ushort)packet.PayloadType);
                    packet.Serialize(writer);
                    int bytesWritten = (int)stream.Position;
                    ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, bytesWritten);
                    SendQueue.Enqueue((segment, remoteEndPoint));
                    Console.WriteLine($"[UCP Session] send to {Socket.RemoteEndPoint}. total {bytesWritten} bytes.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        async Task SendLoopAsync()
        {
            while (IsRunning)
            {
                try
                {
                    while(SendQueue.TryDequeue(out (ArraySegment<byte> segment, EndPoint remoteEndPoint) segmentPair))
                    {
                        try
                        {
                            int bytesSent = await Socket.SendToAsync(segmentPair.segment, segmentPair.remoteEndPoint);
                            Console.WriteLine($"[UCP Session] : Sent data length of {bytesSent}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[UCP Session] : Failed to send data.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // todo -> Catch socket errors
                }
            }
        }

        async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[1 * KB];

            while (IsRunning)
            {
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0); // IPv4 형식

                try
                {
                    SocketReceiveFromResult result = await Socket.ReceiveFromAsync(new ArraySegment<byte>(buffer), senderEndPoint);

                    int bytesRead = result.ReceivedBytes;
                    senderEndPoint = result.RemoteEndPoint;

                    Console.WriteLine($"[UDP Session] received from {senderEndPoint}. total {bytesRead} bytes.");
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
