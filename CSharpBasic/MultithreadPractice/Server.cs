namespace MultithreadPractice
{
    internal class Server : Worker
    {
        public Server(string name, Client client, Barista barista) : base(name)
        {
            ClientCharged = client;
            BaristaAllocated = barista;
        }


        /// <summary>
        /// 담당하게된 손님
        /// </summary>
        internal Client ClientCharged { get; } 

        internal Barista BaristaAllocated { get; }


        internal async Task<SatisfactionType> StartServeAsync()
        {
            Console.WriteLine($"[{Name}] : 손님, 어떤 음료로 하시겠습니까?");
            BeverageType clientResponse = await ClientCharged.GiveMenuAsync<BeverageType>();
            Console.WriteLine($"[{Name}] : 네 알겠습니다. {clientResponse} 로 드릴께요.");
            Beverage beverage = await BaristaAllocated.RequestMakeBeverageAsync(clientResponse);
            Console.WriteLine($"[{Name}] : 손님 주문하신 음료 나왔습니다.");
            SatisfactionType clientSatisfaction = await ClientCharged.GiveBeverageAsync(beverage);

            switch (clientSatisfaction)
            {
                case SatisfactionType.Negative:
                    Console.WriteLine($"[{Name}] : 죄송합니다 손님... 다음부터는 이런일 없도록 하겠습니다...");
                    break;
                case SatisfactionType.Mixed:
                    Console.WriteLine($"[{Name}] : 감사합니다 손님... 다음에는 더 만족할수 있도록 준비하겠습니다.");
                    break;
                case SatisfactionType.Positive:
                    Console.WriteLine($"[{Name}] : 감사합니다 손님 ! 또 오세요.");
                    break;
                default:
                    break;
            }

            return clientSatisfaction;
        }
    }
}
