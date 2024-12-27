using System.Net.Sockets;

namespace ChatServer
{
    public class ServerTcpSession : TcpSession
    {
        public ServerTcpSession(ServerSessionManager manager, Socket socket, int clientId) 
        {
            _manager = manager;
            Socket = socket;
            ClientId = clientId;
        }


        public int ClientId { get; private set; }


        ServerSessionManager _manager;

    }
}
