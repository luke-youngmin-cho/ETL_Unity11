namespace ChatServer
{
    /// <summary>
    /// 클라이언트가 접속할때마다 고유 id 를 부여하기위한 id 생성기
    /// </summary>
    public class ClientIdGenerator
    {
        public ClientIdGenerator(int maxClients)
        {
            _idSet = new HashSet<int>(maxClients);
            _availableIdQueue = new Queue<int>(maxClients);

            for (int i = 1; i <= maxClients; i++)
            {
                _availableIdQueue.Enqueue(i);
            }
        }

        public IEnumerable<int> ClientIds => _idSet;


        readonly HashSet<int> _idSet;
        readonly Queue<int> _availableIdQueue;

        /// <summary>
        /// 새 ID 부여하고 Id Set에 추가
        /// </summary>
        /// <returns> 새로 부여된 ID </returns>
        public int AssignClientId()
        {
            if (_availableIdQueue.Count > 0)
            {
                int id = _availableIdQueue.Dequeue();
                _idSet.Add(id);
                return id;
            }
            else
            {
                Console.WriteLine($"[{nameof(ClientIdGenerator)}] : Server is fulled... reached to max clients.");
                return -1;
            }
        }

        public void ReleaseClientId(int clientId)
        {
            if (_idSet.Remove(clientId))
            {
                _availableIdQueue.Enqueue(clientId);
            }
            else
            {
                // todo -> Something went wrong
            }
        }
    }
}
