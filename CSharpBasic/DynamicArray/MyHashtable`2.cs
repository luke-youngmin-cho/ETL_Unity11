using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
    internal struct KeyValuePair<TKey, TValue>
    {
        internal KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        internal TKey Key;
        internal TValue Value;
    }

    internal class MyHashtable<TKey, TValue>
    {
        internal MyHashtable(int capacity)
        {
            _buckets = new int[capacity];

            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = EMPTY; // 유효하지않은값으로 초기화
            }

            _entries = new Entry[capacity];
            _freeFirstEntryIndex = EMPTY;
        }


        internal TValue this[TKey key]
        {
            get
            {
                // 1. Key 가 null 인지체크
                // 2. Key 로 hashCode 계산, BucketIndex 계산
                // 3. Buckets 에 BucketIndex 접근해서 EntryIndex 가져옴.
                // 4. Entries 에 EntryIndex 접근해서 Entry 가져옴. 
                // 5. Hashcode 같은지 비교 & Entry 의 Key 와 key 가 같은지 비교
                //      같다면 Entry의 Value 반환
                //      아니면 에러코드반환

                if (key == null)
                    throw new ArgumentNullException("Key is null");

                int hashCode = GetHash(key.ToString());
                int bucketIndex = hashCode % _buckets.Length;
                int entryIndex = -1;

                // 요거때매 해시테이블의 탐색 시간복잡도는 O(N)
                for (int i = _buckets[bucketIndex]; i >= 0; i = _entries[i].NextIndex)
                {
                    if (_entries[i].HashCode == hashCode && _entries[i].Key.Equals(key))
                    {
                        entryIndex = i;
                        break;
                    }
                }

                if (entryIndex >= 0)
                    return _entries[entryIndex].Value;
                else
                    throw new KeyNotFoundException();
            }
        }

        internal struct Entry
        {
            internal bool isValid => HashCode >= 0;

            internal int HashCode;
            internal TKey Key;
            internal TValue Value;
            internal int NextIndex;
        }

        int[] _buckets; // Entry 의 시작점 인덱스 참조 배열 [ 1  ㅁ  0 ]
        Entry[] _entries; // 키-밸류 쌍 데이터 저장하는 배열 [ ("A", 3), ("B", 2)]
        int _count;
        const int EMPTY = -1;
        int _freeFirstEntryIndex; // 만들어졌다가 지워져서 재사용가능하게된 Entry중 첫 진입점 인덱스
        int _freeCount; // 삭제되어서 재사용가능하게된 총 Entry수
        
        internal void Add(TKey key, TValue value)
        {
            // 1. Key 중복검사.
            // 2. Key 가 중복 ? 
            //      시작 entry 를 가져와서 빈자리가 나올때까지 탐색

            int hashCode = GetHash(key.ToString()); // key 에 대한 hashcode 생성
            int bucketIndex = hashCode % _buckets.Length; // hashcode 를 capacity 로 mod 해서 bucketIndex 구함

            // buckets 에서 유효한 값은 양수이므로, 유효하지않은 인덱스값이 나올떄까지 반복
            for (int i = _buckets[bucketIndex]; i >= 0; i = _entries[i].NextIndex)
            {
                if (_entries[i].HashCode == hashCode && _entries[i].Key.Equals(key))
                    throw new ArgumentException("The Key already exists.");
            }

            int entryIndex = EMPTY;

            // 재사용가능한 Entry 있는지
            if (_freeCount > 0)
            {
                entryIndex = _freeFirstEntryIndex;
                _freeFirstEntryIndex = _entries[entryIndex].NextIndex; // 첫Entry를 재사용했으므로 그다음 Entry를 첫 Entry로 갱신
                _freeCount--;
            }
            else
            {
                // 추가할 엔트리 공간이 없다면 사이즈 늘림 (동적배열)
                if (_count == _entries.Length)
                {
                    Resize(_count * 2);
                    bucketIndex = hashCode % _buckets.Length; // capacity조정되었으므로 버킷인덱스 다시계산
                }

                entryIndex = _count++;
            }
            
            _entries[entryIndex] = new Entry
            {
                HashCode = hashCode,
                Key = key,
                Value = value,
                NextIndex = _buckets[bucketIndex] 
            };
            _buckets[bucketIndex] = entryIndex;
        }

        internal bool Remove(TKey key)
        {
            // 1. key 가 null 이 아닌지 봐야함
            // 2. Key에 대한 HashCode 및 BucketIndex 계산
            // 3. BucketIndex 에 해당 모든 Entry 순회 
            // 4. 찾았으면 
            //      지우려는게 첫 진입포인트 (Entry) 면 Bucket 의 EntryIndex 갱신
            //      아니면 지우려는 Entry의 다음 EntryIndex 를 지우려는 Entry의 이전 Entry 의 NextEntryIndex 에 대입..
            //      Entry 지움
            //      true 반환
            //    못찾았으면
            //      false반환

            if (key == null)
                throw new ArgumentNullException("key is null");

            int hashCode = GetHash(key.ToString());
            int bucketIndex = hashCode % _buckets.Length;
            int prevEntryIndex = EMPTY;

            for (int i = _buckets[bucketIndex]; i >= 0; prevEntryIndex = i, i = _entries[i].NextIndex)
            {
                if (_entries[i].HashCode == hashCode && _entries[i].Key.Equals(key))
                {
                    if (prevEntryIndex == EMPTY)
                    {
                        _buckets[bucketIndex] = _entries[i].NextIndex;
                    }
                    else
                    {
                        _entries[prevEntryIndex].NextIndex = _entries[i].NextIndex;
                    }

                    _entries[i].HashCode = EMPTY;
                    _entries[i].Key = default;
                    _entries[i].Value = default;
                    _entries[i].NextIndex = _freeFirstEntryIndex;
                    _freeFirstEntryIndex = i; // 현재 Entry 다음에 재사용해야하므로 인덱스 기억
                    _freeCount++;

                    return true;
                }
            }

            return false;
        }

        void Resize(int capacity)
        {
            int[] newBuckets = new int[capacity];

            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = EMPTY;
            }

            Entry[] newEntries = new Entry[capacity];
            Array.Copy(_entries, newEntries, _count);

            for (int entryIndex = 0; entryIndex < _count; entryIndex++)
            {
                // 유효한 엔트리만 BucketIndex 갱신
                if (newEntries[entryIndex].isValid)
                {
                    int bucketIndex = newEntries[entryIndex].HashCode % capacity;
                    newEntries[entryIndex].NextIndex = newBuckets[bucketIndex]; // 버킷인덱스 갱신되었으므로 엔트리의 다음위치도 갱신
                    newBuckets[bucketIndex] = entryIndex;
                }
            }

            _buckets = newBuckets;
            _entries = newEntries;
        }


        // "GI" -> hash? 144
        // "FJ" -> hash? 144
        int GetHash(string name)
        {
            int hash = 0;

            for (int i = 0; i < name.Length; i++)
            {
                hash += name[i];
            }

            return hash;
        }
    }
}
