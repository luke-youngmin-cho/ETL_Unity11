using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    /*
     * Session : 소통에 필요한 정보들을 유지하고있는 기간\
     * UDP (User Datagram Protocol) : 데이터 날리고 끝. 검증 없음
     */
    public class UdpSession
    {
        public UdpSession(int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.Bind(_localEndPoint);
        }


        Socket _socket;
        IPEndPoint _localEndPoint;
        bool _isRunning;
        Task _receiveTask;
        const int MB = 1_024;


        public void Start()
        {
            _isRunning = true;
            _receiveTask = Task.Run(ReceiveLoopAsync);
        }

        async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[1 * MB];

            while (_isRunning)
            {
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0); // IPv4 형식

                try
                {
                    SocketReceiveFromResult result = await _socket.ReceiveFromAsync(new ArraySegment<byte>(buffer), senderEndPoint);

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
