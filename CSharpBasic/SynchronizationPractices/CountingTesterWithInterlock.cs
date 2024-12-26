namespace SynchronizationPractices
{
    internal class CountingTesterWithInterlock : CountingTester
    {
        protected override void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                Interlocked.Increment(ref count);
            }
        }
    }
}
