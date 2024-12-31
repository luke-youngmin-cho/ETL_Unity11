using DebugSystems;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Practices.GameClient.Network.Client
{
    public class ClientUdpSession : UdpSession
    {
        public ClientUdpSession(EndPoint serverEndPoint)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Bind(serverEndPoint);
        }

        protected override void HandleMessage(IMessage message, EndPoint senderEndPoint)
        {
            switch (message.MessageType)
            {
                case MessageType.None:
                    break;
                case MessageType.TcpConnectedResponse:
                    break;
                case MessageType.ChatMessage:
                    break;
                case MessageType.UdpConnectedResponse:
                    HandleUdpConnectedResponse(message);
                    break;
                case MessageType.NetworkObjectTransformUpdate:
                    break;
                default:
                    break;
            }
        }

        void HandleUdpConnectedResponse(IMessage message)
        {
            UdpConnectedResponse udpConnectedResponse = (UdpConnectedResponse)message;
            DebugLogger.Log($"[{nameof(ClientUdpSession)}] {udpConnectedResponse.Message}");
        }
    }
}