using System.Net;
using System.Net.Sockets;
using ChatServer.Network;

namespace ChatServer.Network.Server
{
    public class ServerSessionManager
    {
        public ServerSessionManager(IPAddress ipAddress, int port, int maxClients)
        {
            _serverUdpSession = new ServerUdpSession(this, port);
            _tcpListener = new TcpListener(ipAddress, port);
            _serverTcpSessions = new Dictionary<int, ServerTcpSession>(maxClients);
            _clientIdGenerator = new ClientIdGenerator(maxClients);
        }


        private ServerUdpSession _serverUdpSession;
        private TcpListener _tcpListener;
        private Dictionary<int, ServerTcpSession> _serverTcpSessions;
        private ClientIdGenerator _clientIdGenerator;
        private bool _isRunning;


        public async Task StartAsync()
        {
            Console.WriteLine("Started server sessions");
            _isRunning = true;
            _serverUdpSession.Start();
            _tcpListener.Start(20);

            try
            {
                while (_isRunning)
                {
                    Socket serverTcpSocket = await _tcpListener.AcceptSocketAsync(); // 클라이언트 접속 대기...
                    int clientId = _clientIdGenerator.AssignClientId(); // 클라이언트 접속시 고유 ID 부여

                    // 클라이언트 수가 최대치에 도달했다면
                    if (clientId < 0)
                    {
                        Console.WriteLine($"[{nameof(ServerSessionManager)}] : Server is fulled.");
                        serverTcpSocket.Close(); // 해당클라이언트 응대 하지않고 닫음
                        continue;
                    }

                    ServerTcpSession serverTcpSession = new ServerTcpSession(this, serverTcpSocket, clientId); // 클라이언트 응대 시작

                    serverTcpSession.OnConnected += () =>
                    {
                        TcpConnectedResponse tcpConnectedResponse = new TcpConnectedResponse();
                        tcpConnectedResponse.Message = "[TCP Server] Connected to tcp server.";
                        tcpConnectedResponse.ClientId = clientId;
                        serverTcpSession.Send(tcpConnectedResponse);
                        serverTcpSession.Send(tcpConnectedResponse);

                        UdpConnectedResponse udpConnectedResponse = new UdpConnectedResponse(); // UDP 는 연결개념이 없음.
                        udpConnectedResponse.Message = "[UDP Server] Reached to server.";
                        udpConnectedResponse.ClientId = clientId;
                        _serverUdpSession.Send(udpConnectedResponse, clientId, serverTcpSocket.RemoteEndPoint);
                    };

                    // 클라이언트 연결이 끊어졌을때
                    serverTcpSession.OnDisconnected += () =>
                    {
                        _serverTcpSessions.Remove(clientId); // 더이상 이 서버세션은 관리하지 않음.
                        _clientIdGenerator.ReleaseClientId(clientId); // 담당 클라이언트 ID 반납
                    };

                    _serverTcpSessions.Add(clientId, serverTcpSession); // 이 서버세션 관리
                    serverTcpSession.Connect(serverTcpSocket.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ServerSessionManager)}] Listening Stopped.. {ex} ");
            }
        }


        public void BroadCastTcp(IMessage message)
        {
            foreach (var serverTcpSession in _serverTcpSessions)
            {
                serverTcpSession.Value.Send(message);
            }
        }

        public void MessageToTcp(IMessage message, int senderClientId, int targetClientId)
        {
            if (_serverTcpSessions.ContainsKey(targetClientId))
            {
                _serverTcpSessions[targetClientId].Send(message);
            }
            else
            {
                ChatMessage chatMessage = new ChatMessage();
                chatMessage.SenderClientId = 0; // Server
                chatMessage.Message = $"Failed to send message to {targetClientId}.";
                _serverTcpSessions[senderClientId].Send(chatMessage);
            }
        }

        public void BroadCastUdp(IMessage message)
        {
            foreach (int clientId in _clientIdGenerator.ClientIds)
            {
                EndPoint clientEndPoint = _serverUdpSession.ClientEndPoints[clientId].ClientEndPoint;
                _serverUdpSession.Send(message, clientId, clientEndPoint);
            }
        }

        public void BroadCastUdpToOthers(IMessage message, int excludeCliendId)
        {
            foreach (int clientId in _clientIdGenerator.ClientIds)
            {
                if (clientId == excludeCliendId)
                    continue;

                EndPoint clientEndPoint = _serverUdpSession.ClientEndPoints[clientId].ClientEndPoint;
                _serverUdpSession.Send(message, clientId, clientEndPoint);
            }
        }
    }
}
