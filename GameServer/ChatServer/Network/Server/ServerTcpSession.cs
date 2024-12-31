using System.Net.Sockets;
using ChatServer.Network;

namespace ChatServer.Network.Server
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

        protected override void HandleMessage(IMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.TcpConnectedResponse:
                    break;
                case MessageType.ChatMessage:
                    break;
                default:
                    throw new Exception($"[{nameof(ServerTcpSession)}] Cannot handle the payload {message.MessageType}.");
            }
        }

        void HandleChatMessage(IMessage payload)
        {
            ChatMessage chatMessage = (ChatMessage)payload;

            if (chatMessage.TargetClientId == ChatMessage.TARGET_ALL_CLIENTS)
            {
                _manager.BroadCastTcp(payload);
            }
            else
            {
                _manager.MessageToTcp(payload, ClientId, chatMessage.TargetClientId);
            }
        }
    }
}
