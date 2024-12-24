namespace MultithreadPractice
{
    internal class FrontListener : Worker
    {
        public FrontListener(string name, Barista barista) : base(name)
        {
            _servers = new Dictionary<int, Server>(5);
            BaristaAllocated = barista;
        }


        internal Barista BaristaAllocated { get; }


        internal async Task OnClientCameAsync()
        {
            Client client = new Client(_nextClientId++);
            Server server = new Server($"고객 {client.Id}번 담당", client, BaristaAllocated);
            
            if (_servers.TryAdd(client.Id, server))
            {
                Console.WriteLine($"[{Thread.CurrentThread.Name} 번 쓰레드에서..]손님 {client.Id} 받았습니다.");
                await server.StartServeAsync();
            }
            else
            {
                throw new Exception($"손님 {client.Id} 는 이미 응대중입니다.");
            }
        }

        int _nextClientId;
        Dictionary<int, Server> _servers;
    }
}
