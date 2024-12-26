namespace SynchronizationPractices
{
    internal class CountingTesterWithLock : CountingTester
    {
        object _testLock = new object();

        protected override void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                lock (_testLock)
                {
                    count++;
                }

                //Monitor.Enter(_testLock);
                //// CriticalSection
                //count++;
                //Monitor.Exit(_testLock);
            }
        }
    }
}
