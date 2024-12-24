namespace MultithreadPractice
{
    internal class Client
    {
        internal Client(int id)
        {
            Id = id;
        }

        internal int Id { get; }
        internal BeverageType BeverageTypeOrdered { get; private set; }

        /*
         * 비동기함수 (async)
         * 내부적으로 Task 를 await 하는 문법이기때문에, Task 를 정의하는 함수로 사용하는것임.
         * 그렇기때문에 이 함수를 호출하는 자가 이 함수로 정의된 Task 참조를 반환받을수 있도록해야 상위함수에서 이 Task 를 취급할수있다. 
         * 그렇기때문에 반환타입은 void, Task, Task<T>
         */
        internal async Task<T> GiveMenuAsync<T>()
            where T : Enum
        {
            Console.WriteLine($"[손님{Id}] : (메뉴 뭐 고를지 고민중 ...)");

            // Thread.Sleep (동기 대기) vs Task.Delay (비동기 대기)
            // 동기대기 : 이 Sleep 을 호출한 쓰레드가 멈추게되므로 관련된 상위 쓰레드들이 모두 멈춘다. 
            // 비동기대기 : 이 Thread 를 해제했다가 (컨텍스트는 유지) 다시 복귀시키기 때문에 상위 쓰레드가 멈추지 않는다. 

            // Thread.Sleep(3000);
            await Task.Delay(3000); // await 키워드 : async 함수에서 Task 객체참조 앞에 붙여서 해당 Task 가 완료될때까지 기다림

            Array menuList = Enum.GetValues(typeof(T));
            Random random = new Random();
            int randomIndex = random.Next(menuList.Length);
            T value = (T)menuList.GetValue(randomIndex);

            if (typeof(T) == typeof(BeverageType))            
            {
                BeverageTypeOrdered = (BeverageType)(object)value;
            }
            else
            {
                throw new NotImplementedException();
            }

            Console.WriteLine($"[손님{Id}] : {value} 주세여.");
            return value;
        }

        internal async Task<SatisfactionType> GiveBeverageAsync(Beverage beverage)
        {
            Console.WriteLine($"[손님{Id}] : (음료 {beverage} 마시는중...)");
            await Task.Delay(2000);
            SatisfactionType satisfaction = SatisfactionType.Negative;

            if (beverage.Type == BeverageTypeOrdered)
            {
                Random random = new Random();
                satisfaction = (SatisfactionType)random.Next(1, 3);
            }

            switch (satisfaction)
            {
                case SatisfactionType.Negative:
                    Console.WriteLine($"[손님{Id}] : 내가 주문한건 이게아닌데 ...");
                    break;
                case SatisfactionType.Mixed:
                    Console.WriteLine($"[손님{Id}] : 뭐 나쁘진 않았어요.");
                    break;
                case SatisfactionType.Positive:
                    Console.WriteLine($"[손님{Id}] : 최고에요 !");
                    break;
                default:
                    break;
            }

            return satisfaction;
        }
    }
}
