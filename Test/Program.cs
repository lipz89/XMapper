using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var test = new TestMapper();
            //test.InitConfig();
            //test.Test();

            //test.Test2();

            var t = new TestReadonlyProperty();
            t.Test();

            Console.Read();
        }
    }
}
