namespace MultithreadPractice
{
    internal abstract class Worker
    {
        internal Worker(string name)
        {
            Name = name;
        }


        internal string Name { get; }
    }
}
