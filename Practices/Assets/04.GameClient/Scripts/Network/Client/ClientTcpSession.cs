using DebugSystems;
using System.Net.Sockets;
using UnityEngine;

namespace Practices.GameClient.Network.Client
{
    public class ClientTcpSession : TcpSession
    {
        public ClientTcpSession()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }


        public int ClientId { get; private set; }

        protected override void HandleMessage(IMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.None:
                    break;
                case MessageType.TcpConnectedResponse:
                    HandleTcpConnectedResponse(message);
                    break;
                case MessageType.ChatMessage:
                    break;
                case MessageType.UdpConnectedResponse:
                    break;
                case MessageType.NetworkObjectTransformUpdate:
                    break;
                default:
                    break;
            }
        }

        void HandleTcpConnectedResponse(IMessage message)
        {
            TcpConnectedResponse tcpConnectedResponse = (TcpConnectedResponse)message;
            ClientId = tcpConnectedResponse.ClientId;
            // todo -> 서버연결성공 UI 팝업
            DebugLogger.Log($"[{nameof(ClientTcpSession)}] {tcpConnectedResponse.Message}");
        }

        void HandleChatMessage(IMessage message)
        {
            ChatMessage chatMessage = (ChatMessage)message;
            DebugLogger.Log($"[{nameof(ClientTcpSession)}] {chatMessage.SenderClientId} : {chatMessage.Message}");
        }
    }
}