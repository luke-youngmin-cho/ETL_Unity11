namespace SynchronizationPractices
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CountingTester test1 = new CountingTester();
            Task<CountingTester.CountingTestResult> testTask1 = test1.StartTestAsync(4);
            testTask1.Wait();
            Console.WriteLine($"Test 1 - Count : {testTask1.Result.Count}, ElapsedTime : {testTask1.Result.ElapsedTimeInMS} ms");

            CountingTester test2 = new CountingTesterWithInterlock();
            Task<CountingTester.CountingTestResult> testTask2 = test2.StartTestAsync(4);
            testTask2.Wait();
            Console.WriteLine($"Test 2 - Count : {testTask2.Result.Count}, ElapsedTime : {testTask2.Result.ElapsedTimeInMS} ms");

            CountingTester test3 = new CountingTesterWithSpinLock();
            Task<CountingTester.CountingTestResult> testTask3 = test3.StartTestAsync(4);
            testTask3.Wait();
            Console.WriteLine($"Test 3 - Count : {testTask3.Result.Count}, ElapsedTime : {testTask3.Result.ElapsedTimeInMS} ms");

            CountingTester test4 = new CountingTesterWithLock();
            Task<CountingTester.CountingTestResult> testTask4 = test4.StartTestAsync(4);
            testTask4.Wait();
            Console.WriteLine($"Test 4 - Count : {testTask4.Result.Count}, ElapsedTime : {testTask4.Result.ElapsedTimeInMS} ms");

            CountingTester test5 = new CountingTesterWithMutex();
            Task<CountingTester.CountingTestResult> testTask5 = test5.StartTestAsync(4);
            testTask5.Wait();
            Console.WriteLine($"Test 5 - Count : {testTask5.Result.Count}, ElapsedTime : {testTask5.Result.ElapsedTimeInMS} ms");

            CountingTester test6 = new CountingTesterWithSemaphore();
            Task<CountingTester.CountingTestResult> testTask6 = test6.StartTestAsync(4);
            testTask6.Wait();
            Console.WriteLine($"Test 6 - Count : {testTask6.Result.Count}, ElapsedTime : {testTask6.Result.ElapsedTimeInMS} ms");
        }
    }
}
