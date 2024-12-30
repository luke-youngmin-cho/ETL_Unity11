using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
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

                    // 클라이언트 연결이 끊어졌을때
                    serverTcpSession.OnDisconnected += () =>
                    {
                        _serverTcpSessions.Remove(clientId); // 더이상 이 서버세션은 관리하지 않음.
                        _clientIdGenerator.ReleaseClientId(clientId); // 담당 클라이언트 ID 반납
                    };

                    _serverTcpSessions.Add(clientId, serverTcpSession); // 이 서버세션 관리
                }
            }
            catch
            {

            }
        }


        public void BroadCastTcp(IPayload payload)
        {
            foreach (var serverTcpSession in _serverTcpSessions)
            {
                serverTcpSession.Value.Send(payload);
            }
        }

        public void BroadCastUdp(IPayload payload)
        {
            foreach (int clientId in _clientIdGenerator.ClientIds)
            {
                EndPoint clientEndPoint = _serverUdpSession.ClientEndPoints[clientId].ClientEndPoint;
                _serverUdpSession.Send(payload, clientId, clientEndPoint);
            }
        }

        public void BroadCastUdpToOthers(IPayload payload, int excludeCliendId)
        {
            foreach (int clientId in _clientIdGenerator.ClientIds)
            {
                if (clientId == excludeCliendId)
                    continue;

                EndPoint clientEndPoint = _serverUdpSession.ClientEndPoints[clientId].ClientEndPoint;
                _serverUdpSession.Send(payload, clientId, clientEndPoint);
            }
        }
    }
}
