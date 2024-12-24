namespace MultithreadPractice
{
    internal class Beverage
    {
        internal Beverage(BeverageType type)
        {
            Type = type;
        }


        internal BeverageType Type { get; }


        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
