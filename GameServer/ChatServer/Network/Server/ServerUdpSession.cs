using System.Net;
using System.Net.Sockets;
using ChatServer.Network;
using static ChatServer.Network.Server.ServerSettings;

namespace ChatServer.Network.Server
{
    public class ServerUdpSession : UdpSession
    {
        public ServerUdpSession(ServerSessionManager manager, int port)
        {
            _manager = manager;
            _port = port;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, _port));
            ClientEndPoints = new ClientEndPointPair[MAX_CLIENT + 1];
        }


        public struct ClientEndPointPair
        {
            public ClientEndPointPair(bool isValid, EndPoint clientEndPoint)
            {
                IsValid = isValid;
                ClientEndPoint = clientEndPoint;
            }


            public bool IsValid;
            public EndPoint ClientEndPoint;
        }

        public ClientEndPointPair[] ClientEndPoints { get; private set; }


        ServerSessionManager _manager;
        int _port;


        public void Send(IMessage payload, int clientId, EndPoint remoteEndPoint)
        {
            if (ClientEndPoints[clientId].IsValid == false)
            {
                ClientEndPoints[clientId].IsValid = true;
                ClientEndPoints[clientId].ClientEndPoint = remoteEndPoint;
            }

            InternalSend(payload, remoteEndPoint);
        }

        protected override void HandleMessage(IMessage payload, EndPoint senderEndPoint)
        {
            switch (payload.MessageType)
            {
                case MessageType.NetworkObjectTransformUpdate:
                    HandleNetworkObjectTransformUpdate(payload, senderEndPoint);
                    break;
                default:
                    throw new NotImplementedException($"[{nameof(ServerUdpSession)}] Cannot handle the payload type {payload.MessageType}.");
            }
        }

        void HandleNetworkObjectTransformUpdate(IMessage payload, EndPoint senderEndPoint)
        {
            _manager.BroadCastUdp(payload);
        }
    }
}
