using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortAlgorithm
{
    internal static class ArrayExtensions
    {
        /// <summary>
        /// 거품정렬
        /// O(N^2)
        /// 특징 : 순회 한번 할때마다 마지막 자리가 고정.
        /// Stable.(중복값이 있을때 정렬 전의 순서가 정렬 후에도 보장됨)
        /// </summary>
        /// <param name="arr"></param>
        internal static void BubbleSort(this int[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - 1 - i; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        Swap(ref arr[j], ref arr[j + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// 선택정렬
        /// O(N^2)
        /// 특징 : 순회할때마다 가장 작은값이 고정
        /// Unstable.
        /// </summary>
        /// <param name="arr"></param>
        internal static void SelectionSort(this int[] arr)
        {
            int i, j, minIdx = 0;

            for (i = 0; i < arr.Length - 1; i++)
            {
                minIdx = i;

                for (j = i + 1; j < arr.Length; j++)
                {
                    if (arr[j] < arr[minIdx])
                        minIdx = j;
                }

                Swap(ref arr[i], ref arr[minIdx]);
            }
        }

        /// <summary>
        /// 삽입 정렬
        /// O(N^2)
        /// Stable/Unstable 여부는 같은값을 스왑할지 여부에따라 달라짐
        /// </summary>
        /// <param name="arr"></param>
        internal static void InsertionSort(this int[] arr)
        {
            int key; // 얘는 인덱스 아니고 값임

            for (int i = 1; i < arr.Length; i++)
            {
                key = arr[i];
                int j = i - 1;

                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = key;
            }
        }


        public static void MergeSort(this int[] arr)
        {
            int length = arr.Length;

            for (int mergeSize = 1; mergeSize < length; mergeSize *= 2)
            {
                for (int start = 0; start < length; start += 2 * mergeSize)
                {
                    //왼쪽: start ~ mergeSize -1
                    //오른쪽: start ~ 2*mergeSize -1
                    // 다음파티션 시작위치 : start + 2 * mergeSize
                    int mid = Math.Min(start + mergeSize - 1, length - 1);
                    int end = Math.Min(start + 2 * mergeSize - 1, length - 1);

                    Merge(arr, start, mid, end);
                }
            }
        }

        public static void RecursiveMergeSort(this int[] arr)
        {
            Internal_RecursiveMergeSort(arr, 0, arr.Length - 1);
        }

        private static void Internal_RecursiveMergeSort(int[] arr, int start, int end)
        {
            if (start < end)
            {
                // 코딩테스트할때는 연산순서를  -, / 이후 +, * 하는 형태의 연산 스타일을 지향해야 Overflow 를 최대한 방지할수있다.
                int mid = end + (start - end + 1) / 2 - 1; //(start + end) / 2; => (start - end) / 2 + end
                Internal_RecursiveMergeSort(arr, start, mid); // 왼쪽 Partition
                Internal_RecursiveMergeSort(arr, mid + 1, end); // 오른쪽 Partition

                Merge(arr, start, mid, end);
            }
        }

        private static void Merge(int[] arr, int start, int mid, int end)
        {
            int length1 = mid - start + 1; // Part1 참조 배열 길이
            int length2 = end - (mid + 1) + 1; // Part2 참조 배열 길이
            int[] copy1 = new int[length1]; // Part1 참조용 배열
            int[] copy2 = new int[length2]; // Part2 참조용 배열

            for (int i = 0; i < length1; i++)
                copy1[i] = arr[start + i];

            for (int i = 0; i < length2; i++)
                copy2[i] = arr[mid + 1 + i];

            int part1 = 0;
            int part2 = 0;
            int index = start;

            while (part1 < length1 && part2 < length2)
            {
                // 현재 인덱스에서 part1 이 part2 이하면 part1 값을 채택하여 적용
                if (copy1[part1] <= copy2[part2])
                    arr[index++] = copy1[part1++];
                else
                    arr[index++] = copy2[part2++];
            }

            // 선정되지못하고 남은 part1 참조배열의 모든 아이템을 index 위치에 덮어씀
            while (part1 < length1)
                arr[index++] = copy1[part1++];
        }



        public static void QuickSort(this int[] arr)
        {
            Stack<(int start, int end)> partitionStack = new Stack<(int, int)>();
            partitionStack.Push((0, arr.Length - 1));

            while (partitionStack.Count > 0)
            {
                (int start, int end) partition = partitionStack.Pop();
                int fixedIndex = SplitPartition(arr, partition.start, partition.end);

                // 왼쪽으로 나눌게 있는지
                if (fixedIndex - 1 > partition.start)
                {
                    partitionStack.Push((partition.start, fixedIndex - 1));
                }
                
                // 오른쪽으로 나눌게 있는지
                if (fixedIndex + 1 < partition.end)
                {
                    partitionStack.Push((fixedIndex + 1, partition.end));
                }
            }
        }

        private static int SplitPartition(int[] arr, int start, int end)
        {
            int mid = end + (start - end + 1) / 2 - 1;
            int pivot = arr[mid];

            while (true)
            {
                while (arr[start] < pivot) start++;
                while (arr[end] > pivot) end--;

                if (start < end)
                {
                    if (arr[start] == pivot && arr[end] == pivot)
                        end--;
                    else
                        Swap(ref arr[start], ref arr[end]);
                }
                else
                {
                    return end;
                }
            }
        }

        public static void HeapSort(this int[] arr)
        {
            HeapifyBottonUp(arr);
            InverseHeapify(arr);
        }

        static void InverseHeapify(int[] arr)
        {
            int end = arr.Length - 1;

            while (end > 0)
            {
                Swap(ref arr[0], ref arr[end--]);
                SIFTDown(arr, end, 0);
            }
        }

        static void HeapifyTopDown(int[] arr)
        {
            int current = 1;

            while (current < arr.Length)
            {
                SIFTUp(arr, current++);
            }
        }

        static void HeapifyBottonUp(int[] arr)
        {
            int current = arr.Length - 1;

            while (current >= 0)
            {
                SIFTDown(arr, arr.Length - 1, current--);
            }
        }

        static void SIFTUp(int[] arr, int current)
        {
            int parent = (current - 1) / 2; // 부모노드 인덱스 계산

            // 정렬하려는 노드의 인덱스가 루트보다 크다면 반복
            while (current > 0)
            {
                if (arr[current].CompareTo(arr[parent]) > 0)
                {
                    Swap(ref arr[current], ref arr[parent]);
                    current = parent;
                    parent = (current - 1) / 2; // <- 추가해주삼 
                }
                else
                {
                    break;
                }
            }
        }

        static void SIFTDown(int[] arr, int end, int current)
        {
            int leftChild = (current * 2) + 1;

            while (leftChild <= end)
            {
                int priorityChild = leftChild;
                int rightChild = leftChild + 1;

                // 오른쪽 자식이 존재하면서 왼쪽자식보다 더 우세하다면 오른쪽자식을 비교대상으로 삼음
                if (rightChild <= end &&
                    arr[rightChild].CompareTo(arr[leftChild]) > 0)
                {
                    priorityChild = rightChild;
                }

                // current 값과 child 값을 비교해서 current 가 더 작으면(우선순위가 낮으면) 스왑.
                if (arr[current].CompareTo(arr[priorityChild]) < 0)
                {
                    Swap(ref arr[current], ref arr[priorityChild]);
                    current = priorityChild;
                    leftChild = (current * 2) + 1;
                }
                else
                {
                    break;
                }
            }
        }

        static void Swap(ref int a, ref int b)
        {
            int tmp = a;
            a = b;
            b = tmp;
        }
    }
}
