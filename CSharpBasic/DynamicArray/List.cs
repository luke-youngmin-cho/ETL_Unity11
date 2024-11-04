
using System.Collections;

namespace DynamicArray
{
    internal class List : IEnumerable<int>
    {
        private const int DEFAULT_SIZE = 4;
        private int[] _data = new int[DEFAULT_SIZE];
        private int _count = 0;
        public delegate bool FindIndexCondition(int item);

        public struct Enumerator : IEnumerator<int>
        {
            public Enumerator(List list)
            {
                _list = list;
            }

            private List _list;

            public int Current => _list._data[_index]; // 현재 페이지

            object IEnumerator.Current => _list._data[_index]; // 현재 페이지

            private int _index = -1; // <= 현재페이지 인덱스

            /// <summary>
            /// 다음 아이템을 가리키는 인덱스로 넘어가기를 시도
            /// </summary>
            /// <returns> 다음 값이 있으면 true </returns>
            public bool MoveNext()
            {
                // 아직 넘길 페이지 남음
                if (_index < _list._count - 1)
                {
                    _index++;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
            }
        }


        public void Add(int item)
        {
            // 용량 부족하니 두배로 늘림
            if (_count >= _data.Length)
            {
                int[] tmpArr = new int[_data.Length * 2];
                for (int i = 0; i < _data.Length; i++)
                {
                    tmpArr[i] = _data[i];
                }
                _data = tmpArr;
            }

            _data[_count++] = item;
        }

        public int FindIndex(int item)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] == item)
                    return i;
            }

            return -1;
        }

        public int FindIndex(FindIndexCondition condition)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                if (condition.Invoke(_data[i]))
                    return i;
            }

            return -1;
        }


        public void Delete(int item)
        {
            int index = FindIndex(item);

            if (index < 0)
            {
                return;
            }

            for (int i = index; i < _data.Length - 1; i++)
            {
                _data[i] = _data[i + 1];
            }

            _count--;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
