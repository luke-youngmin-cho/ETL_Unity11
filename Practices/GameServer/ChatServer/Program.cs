using System.Net;

namespace ChatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Any;
            ServerSessionManager serverSessionManager = new ServerSessionManager(ipAddress, 9000, 100);
            Task serverSessionTask = serverSessionManager.StartAsync();
            serverSessionTask.Wait();
        }
    }
}
