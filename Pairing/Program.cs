using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairing
{
    class Program
    {
        private static readonly Pairing<int,string> pairing = new Pairing<int, string>();
        static void Main(string[] args)
        {

            

            Thread firsThread = new Thread( ()=> Console.WriteLine(pairing.Provide(5, 999999999)));
            Thread second = new Thread( ()=> Console.WriteLine(pairing.Provide("cinco", 999999999)));

            Thread third = new Thread(() => Console.WriteLine(pairing.Provide(6, 999999999)));
            Thread fourth = new Thread(() => Console.WriteLine(pairing.Provide("seis", 999999999)));

            Thread fifth = new Thread(() => Console.WriteLine(pairing.Provide(7, 999999999)));
            Thread sixth = new Thread(() => Console.WriteLine(pairing.Provide("sete", 999999999)));


            second.Start();
            firsThread.Start();
            third.Start();
            fourth.Start();
            fifth.Start();
            sixth.Start();

            Console.ReadKey();

        }

        public static void func()
        {
           Console.WriteLine( pairing.Provide(5, 200));

        }
    }
}
