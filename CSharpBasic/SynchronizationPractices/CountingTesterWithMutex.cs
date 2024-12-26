namespace SynchronizationPractices
{
    internal class CountingTesterWithMutex : CountingTester
    {
        Mutex _mutex = new Mutex(false);

        protected override void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                try
                {
                    _mutex.WaitOne();
                    count++;
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }
    }
}
