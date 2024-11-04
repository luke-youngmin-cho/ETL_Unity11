namespace DynamicArray
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List list = new List();
            list.Add(3);
            list.Add(4);
            list.Add(1);
            list.Add(7);

            int indexOf4 = list.FindIndex(4);
            int indexOfLessThan2 = list.FindIndex(x => x < 2);
            int indexOfMoreThan6 = list.FindIndex(x => x > 6);

            List.Enumerator e = new List.Enumerator(list);

            while (e.MoveNext())
            {
                Console.WriteLine(e.Current); 
            }

            foreach (var item in list)
            {
            }
        }

        public bool LessThan2(int a)
        {
            return a < 2;
        }
    }
}
