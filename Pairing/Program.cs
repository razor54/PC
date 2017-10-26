using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairing
{
    class Program
    {
        private static readonly Pairing2<int, string> pairing = new Pairing2<int, string>();
        private static int DEBUG = 999999999;
        private static int TEST = 100;


        static void Main(string[] args)
        {
            int currConfig = DEBUG;

            Thread firsThread = new Thread(() => Console.WriteLine(pairing.Provide(5, currConfig)));
            Thread second = new Thread(() => Console.WriteLine(pairing.Provide("cinco", currConfig)));

            Thread third = new Thread(() => Console.WriteLine(pairing.Provide(6, currConfig)));
            Thread fourth = new Thread(() => Console.WriteLine(pairing.Provide("seis", currConfig)));

            Thread fifth = new Thread(() => Console.WriteLine(pairing.Provide(7, currConfig)));
            Thread sixth = new Thread(() => Console.WriteLine(pairing.Provide("sete", currConfig)));

            firsThread.Start();
            second.Start();
            third.Start();
            fourth.Start();
            fifth.Start();

            sixth.Start();


            Console.ReadKey();
        }

        public static void func()
        {
            Console.WriteLine(pairing.Provide(5, 200));
        }
    }
}
