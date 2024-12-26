namespace MultithreadPractice
{
    /*
     * 멀티쓰레드
     * CPU Bound 작업 : CPU 연산량 자체가 많은 작업을 여러 코어에 분산해서 수행할수있도록 함. (코어갯수에 비례하게 할당)
     * IO Bound 작업 : 실시간으로 요청에 대한 응답을 기다려야하는 작업. (응답을 계속 대기해야하므로 갯수에 크게 신경쓰지않음..그렇다고 많이 만들라는건아님)
     * 메인쓰레드 : 프로세스가 처음 시작되었을때 메인함수를 할당하는 쓰레드.
     * 멀티쓰레드환경에서는 스케쥴러가 내부 알고리즘으로 최적의 상황을 고려하여 작업을 CPU 에 스케쥴링 하므로, 먼저 할당한 쓰레드가 먼저 결과를 반환하지는 않는다.
     * ThreadPool : 쓰레드가 필요할때마다 만들었다가 삭제하면 발생하는 비용이 크므로, 풀링을 통해 오버헤드를 최소화한다.
     * async-await : Task 기반 비동기문법. 어떤 작업이 할당된 쓰레드를 기다리는동안 현재 쓰레드에서 다른 작업을 이어서 진행할수있음.
     */
    internal class Program
    {
        static void Main(string[] args)
        {
            //Thread thread1 = new Thread(() => SayHi());
            //thread1.Name = "DummyThread1";
            //thread1.IsBackground = true; // Foreground 스레드가 모두 종료되면 자동 종료된다.
            //thread1.Start();

            //Test();

            //Thread thread2 = new Thread(SayHi);
            //thread2.Name = "DummyThread2";
            //thread2.IsBackground = false;
            //thread2.Start();

            //ThreadPool.SetMinThreads(1, 0); // workerThreads : CPU Bound 작업을 위한 쓰레드
            //ThreadPool.SetMaxThreads(4, 3); // completionPortThreads : IO Bound 작업을 위한 쓰레드
            //ThreadPool.QueueUserWorkItem(SayHi);

            // Task : .Net 환경에 의해 관리되는 ThreadPool 에서 최적의 알고리즘으로 작업을 할당하는 클래스

            //Task task1 = new Task(SayHi);
            //task1.Start();

            Task cafeTask = OpenCafeAsync();
            cafeTask.Wait();
        }

        static async void Test()
        {
            Task task1 = new Task(SayHi);
            task1.Start();
            //task1.Wait();
            //await Task.Delay(1000);
            await task1;

            Console.WriteLine("테스트완료.");
        }

        static async Task OpenCafeAsync()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Barista barista = new Barista("커피장인");
            FrontListener frontListener = new FrontListener("문지기", barista);

            int clientCount = 4;
            List<Task> clientServingTasks = new List<Task>(clientCount);

            for (int i = 0; i < clientCount; i++)
            {
                Task task = Task.Factory.StartNew(async () =>
                {
                    await frontListener.OnClientCameAsync();
                }, cts.Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default); // 멀티쓰레드

                //Task task = frontListener.OnClientCameAsync(); // 싱글쓰레드
                clientServingTasks.Add(task);
            }

            cts.Cancel();
            try
            {
                await Task.WhenAll(clientServingTasks); // 모든 고객 응대 끝날때까지 대기
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("카페 문 급히 닫음");
            }
        }

        static void SayHi()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"{Thread.CurrentThread.Name} says hi {i} times.");
            }
        }

        static void SayHi(object state)
        {
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"{Thread.CurrentThread.Name} says hi {i} times.");
            }
        }
    }
}
