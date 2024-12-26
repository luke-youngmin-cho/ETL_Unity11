using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronizationPractices
{
    internal class CountingTester
    {
        internal struct CountingTestResult
        {
            internal int Count;
            internal long ElapsedTimeInMS;
        }


        protected int count;


        internal async Task<CountingTestResult> StartTestAsync(int totalTask)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<Task> tasks = Enumerable
                .Range(0, totalTask)
                .Select(_ => Task.Run(IncrementCounter));
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            return new CountingTestResult { Count = count, ElapsedTimeInMS = stopwatch.ElapsedMilliseconds };
        }

        protected virtual void IncrementCounter()
        {
            for (int i = 0; i < 500000; i++)
            {
                count++;
            }
        }
    }
}
