using System.Diagnostics;
using System.Linq; // Linq : Enumerable 들에 대한 확장함수를 제공하는 namespace

namespace SortAlgorithm
{
    internal class Program
    {
        static void Main(string[] args)
        {

            int[] arr = { 1, 4, 3, 3, 9, 8, 7, 2, 5, 0 };

            Random random = new Random();

            arr =
            Enumerable.Repeat(0, 1000000)
                      .Select(x => random.Next(0, 100))
                      .ToArray();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //arr.BubbleSort(); // 2_765_915 MS
            //arr.SelectionSort(); // 905_949 MS
            //arr.InsertionSort(); // 531_221 MS
            //arr.RecursiveMergeSort(); // 194 MS
            //arr.MergeSort(); // 183 MS
            //arr.QuickSort(); // 160 MS, 중복값이많으면 성능 떨어짐
            arr.HeapSort(); // 325 MS, 중복값 많으면 오히려 성능 좋음
            // C# 의 List.Sort 가 삽입, 퀵, 힙 소트를 섞어쓰는이유
            // 자료갯수가 많이 적다면 퀵/힙같은 복잡한 알고리즘을 수행하는 오버헤드가 더 비싸기때문에 삽입정렬 채택
            // 보통의 경우는 QuickSort 를 사용
            // Quick 단점은 파티션이 잘 분할되지 않으면 O(N^2) 시간복잡도로 성능이 많이 저하될 수 있으므로 
            // 파티션이 많아지면 HeapSort 로 전환.

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} MS");
            //PrintAllItmes(arr);
        }

        static void PrintAllItmes(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                Console.Write($"{arr[i]}, ");
            }
        }
    }
}
