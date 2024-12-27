using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    public class ServerUdpSession : UdpSession
    {
        public ServerUdpSession(ServerSessionManager manager, int port)
        {
            _manager = manager;
            _port = port;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, _port));
        }


        ServerSessionManager _manager;
        int _port;
    }
}
