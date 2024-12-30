using System.Net;
using static ChatServer.ServerSettings;

namespace ChatServer
{
    internal class Program
    {
        


        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Any;
            ServerSessionManager serverSessionManager = new ServerSessionManager(ipAddress, PORT, MAX_CLIENT);
            Task serverSessionTask = serverSessionManager.StartAsync();
            serverSessionTask.Wait();
        }
    }
}
