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
            _serverTcpSessions = new Dictionary<int, TcpSession>(maxClients);
            _clientIdGenerator = new ClientIdGenerator(maxClients);
        }


        private UdpSession _serverUdpSession;
        private TcpListener _tcpListener;
        private Dictionary<int, TcpSession> _serverTcpSessions;
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
                    Socket serverTcpSocket = await _tcpListener.AcceptSocketAsync();
                    int clientId = _clientIdGenerator.AssignClientId();

                    if (clientId < 0)
                    {
                        Console.WriteLine($"[{nameof(ServerSessionManager)}] : Server is fulled.");
                        serverTcpSocket.Close();
                        continue;
                    }

                    ServerTcpSession serverTcpSession = new ServerTcpSession(this, serverTcpSocket, clientId);
                    serverTcpSession.OnDisconnected += () =>
                    {
                        _serverTcpSessions.Remove(clientId);
                        _clientIdGenerator.ReleaseClientId(clientId);
                    };
                    _serverTcpSessions.Add(clientId, serverTcpSession);
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
    }
}
