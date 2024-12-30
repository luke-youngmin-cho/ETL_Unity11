using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    /*
     * Session : 소통에 필요한 정보들을 유지하고있는 기간
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
        protected Task SendTask; // 송신
        protected Task ReceiveTask; // 수신
        protected const int KB = 1_024;
        protected Queue<(ArraySegment<byte> segment, EndPoint remoteEndPoint)> SendQueue; // 송신대기열 (어떤데이터를 누구한테 보낼건지 에 대한 목록)
        protected readonly ArrayPool<byte> BufferPool; // 송/수신시 사용하는 버퍼의 생성/해제 비용을 최소화하기위함.


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

        /// <summary>
        /// 송신 대기열에 송신할 데이터를 등록
        /// </summary>
        /// <param name="payload"> 송신할 데이터 </param>
        /// <param name="remoteEndPoint"> 데이터를 받아야할 이용자(클라이언트) </param>
        protected void InternalSend(IPayload payload, EndPoint remoteEndPoint)
        {
            byte[] buffer = BufferPool.Rent(1 * KB);

            try
            {
                using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, true))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((ushort)payload.PayloadType);
                    payload.Serialize(writer);
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

        /// <summary>
        /// 계속 순회하면서 송신 대기열에 송신할 데이터가 있는지 확인후 송신.
        /// </summary>
        async Task SendLoopAsync()
        {
            while (IsRunning)
            {
                try
                {
                    // 송신할 데이터가 있으면 계속 이어서 송신
                    while(SendQueue.TryDequeue(out (ArraySegment<byte> segment, EndPoint remoteEndPoint) segmentPair))
                    {
                        try
                        {
                            int bytesSent = await Socket.SendToAsync(segmentPair.segment, segmentPair.remoteEndPoint); // 데이터 송신
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

        /// <summary>
        /// UDP 데이터 수신이 들어올때까지 대기하는것을 반복수행
        /// </summary>
        async Task ReceiveLoopAsync()
        {
            byte[] buffer = BufferPool.Rent(1 * KB);

            while (IsRunning)
            {
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0); // IPv4 형식 아무나 (UDP 는 1:1 대응이아니고 무작위로 프로토콜 맞으면 데이터 송수신 되기때문)

                try
                {
                    SocketReceiveFromResult result = await Socket.ReceiveFromAsync(new ArraySegment<byte>(buffer), senderEndPoint); // 누구든지 UDP 로 데이터를 나한테 보낸 클라이언트가 있을때까지 기다림

                    int bytesRead = result.ReceivedBytes; // 수신한 데이터 바이트수
                    senderEndPoint = result.RemoteEndPoint; // 데이터를 송신한 클라이언트

                    // 수신 데이터 길이가 타입데이터길이보다 짧으면 패킷에 문제가있는것임
                    if (bytesRead < sizeof(PayloadType))
                    {
                        Console.WriteLine($"[{nameof(UdpSession)}] Received invalid data.");
                        continue;
                    }

                    using (MemoryStream stream = new MemoryStream(buffer, 0, bytesRead))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        PayloadType payloadType = (PayloadType)reader.ReadUInt16(); // 젤앞에 ushort 는 payloadtype 으로 송신되었을것이므로 수신시에도 동일하게 처리
                        IPayload payload = PayloadFactory.Create(payloadType);

                        // 길이는 올바르지만 PayloadType 이 잘못 들어왔을수도 있으므로(Payload 객체만드는데 실패했을것임) 확인
                        if (payload != null)
                        {
                            payload.Deserialize(reader); // Payload 객체를 수신받은 데이터로 초기화
                            HandlePayload(payload, senderEndPoint); // Payload 를 취급하는 구현부에서 작성된 내용대로 처리할것.
                        }
                        else
                        {
                            Console.WriteLine($"[{nameof(UdpSession)}] Received invalid payload type.");
                        }
                    }

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

        /// <summary>
        /// 수신받은 데이터를 처리
        /// </summary>
        /// <param name="payload"> 수신받은 데이터 </param>
        /// <param name="senderEndPoint"> 데이터를 송신한 이용자 (클라이언트) </param>
        protected abstract void HandlePayload(IPayload payload, EndPoint senderEndPoint);
    }
}
