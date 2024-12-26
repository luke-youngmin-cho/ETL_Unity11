namespace SynchronizationPractices
{
    internal class CountingTesterWithSemaphore : CountingTester
    {
        Semaphore _semaphore = new Semaphore(1, 1);

        protected override void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                try
                {
                    _semaphore.WaitOne();
                    count++;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}
