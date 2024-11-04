using System;

namespace ARP.Test.As
{
    public class A
    {
        public A(int age)
        {
            _age = age;
        }

        private int _age;

        public void SayMyAge()
        {
            Console.WriteLine(this._age);
        }
    }
}