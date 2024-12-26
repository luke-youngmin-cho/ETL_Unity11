namespace SynchronizationPractices
{
    internal class CountingTesterWithSpinLock : CountingTester
    {
        SpinLock _spinLock = new SpinLock();


        protected override void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                bool lockTaken = false;

                // spinlock 을 쓸때, try-finally 구문을 사용하는 이유는 
                // Critical Section 에서 예외가 던져지더라도, CriticalSection 을 해제하는 구문 (Exit) 이
                // 실행될 수 있도록 함.
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    count++;
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit();
                }
            }
        }
    }
}
