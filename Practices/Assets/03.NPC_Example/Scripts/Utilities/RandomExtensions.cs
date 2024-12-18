using System;
using System.Collections;
using System.Collections.Generic;

namespace Practices.NPC_Example.Utilities
{
    public static class RandomExtensions
    {
        public static void Shuffle(this Random random, IList list)
        {
            int n = list.Count;

            while (n > 1)
            {
                int m = random.Next(0, n--); // 0 ~ n-1 사이 난수 생성 (n -1 이하의 인덱스들 중에 랜덤하게 선택)
                object tmp = list[m];
                list[m] = list[n];
                list[n] = tmp;
            }
        }

        public static void Shuffle<T>(this Random random, IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                int m = random.Next(0, n--); // 0 ~ n-1 사이 난수 생성 (n -1 이하의 인덱스들 중에 랜덤하게 선택)
                T tmp = list[m];
                list[m] = list[n];
                list[n] = tmp;
            }
        }
    }
}