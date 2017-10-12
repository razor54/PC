using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Trabalho_PC
{
    internal class Class1
    {
        public static void Main(string[] args)
        {
            Test1();
            Console.ReadKey();
        }


        private static readonly ExpirableLazy<string> expirable = new ExpirableLazy<string>(
            () =>
            {
                Thread.Sleep(7);
                return 4.ToString() + "--" + Thread.CurrentThread.ManagedThreadId.ToString();
            },
            new TimeSpan(0, 0, 0, 0 , 8)
        );

       
        public static void Test1()
        {
            Thread firtsThread = new Thread(fun);
            Thread secondThread = new Thread(fun);
            Thread thirdThread = new Thread(fun);
            Thread thread4 = new Thread(fun);
            Thread thread5 = new Thread(fun);
            Thread thread6 = new Thread(fun);
            Thread thread7 = new Thread(fun);
            Thread thread8 = new Thread(fun);
            Thread thread9 = new Thread(fun);
            Thread thread10 = new Thread(fun);
            
            firtsThread.Start();
            secondThread.Start();
            thirdThread.Start();
            thread4.Start();
            thread5.Start();
            thread6.Start();
            thread7.Start();
            thread8.Start();
            thread9.Start();
            thread10.Start();

        }

        private static void fun()
        {
            Thread.Sleep(500);
            var expirableValue = expirable.Value;
            
            Console.WriteLine(expirableValue);

        }
    }
}
