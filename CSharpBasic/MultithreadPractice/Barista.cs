namespace MultithreadPractice
{
    internal class Barista : Worker
    {
        public Barista(string name) : base(name)
        {
        }


        internal async Task<Beverage> RequestMakeBeverageAsync(BeverageType beverageType)
        {
            Console.WriteLine($"[{Name}] : 음료 {beverageType} 제작중...");
            await Task.Delay(5000);

            Array menuList = Enum.GetValues(typeof(BeverageType));
            Random random = new Random();
            int randomIndex = random.Next(menuList.Length);
            BeverageType value = (BeverageType)menuList.GetValue(randomIndex);
            Beverage beverage = new Beverage(value);
            Console.WriteLine($"[{Name}] : 음료 {beverage} 제작완료.");
            
            return beverage;
        }
    }
}
