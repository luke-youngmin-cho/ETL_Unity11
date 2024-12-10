using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicArray
{
    internal class PriorityQueue<T>
    {
        internal PriorityQueue()
        {
            _heap = new List<T>();
        }

        internal PriorityQueue(int capacity)
        {
            _heap = new List<T>(capacity);
        }

        List<T> _heap;

        internal T Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Priority queue is empty");

            return _heap[0];
        }

        internal void Enqueue(T value)
        {

        }

        void SIFTUp()
        {

        }
    }
}
