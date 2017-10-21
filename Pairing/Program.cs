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
        private static readonly Pairing<int,string> pairing = new Pairing<int, string>();

        static void Main(string[] args)
        {

            
            
            Thread firsThread = new Thread( ()=> Console.WriteLine(pairing.Provide(5,100 )));
            Thread second = new Thread( ()=> Console.WriteLine(pairing.Provide("cinco", 100)));
           
            Thread third = new Thread(() => Console.WriteLine(pairing.Provide(6, 100)));
            Thread fourth = new Thread(() => Console.WriteLine(pairing.Provide("seis", 100)));

            Thread fifth = new Thread(() => Console.WriteLine(pairing.Provide(7, 100)));
            Thread sixth = new Thread(() => Console.WriteLine(pairing.Provide("sete", 100)));
            
    
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
           Console.WriteLine( pairing.Provide(5, 200));

        }
    }
}
