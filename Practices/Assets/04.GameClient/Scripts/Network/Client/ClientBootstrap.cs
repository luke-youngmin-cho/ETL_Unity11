using DebugSystems;
using UnityEngine;

namespace Practices.GameClient.Network.Client
{
    public class ClientBootstrap
    {
        /// <summary>
        /// 
        /// </summary>
        [RuntimeInitializeOnLoadMethod] // 처음 어플리케이션 시작시 호출
        public static void Start()
        {
            DebugLogger.Log("Client start");
            clientSessionManager = new ClientSessionManager();
            clientSessionManager.Start();
        }

        public static ClientSessionManager clientSessionManager;
    }
}