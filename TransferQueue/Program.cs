using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TransferQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            test8();
            Thread.Sleep(50);

            Console.ReadKey();
        }

        private static readonly TransferQueue<string> _queue = new TransferQueue<string>();

        public static bool test8()
        {
            queue = new ConcurrentQueue<string>();

            Thread thread2 = new Thread((() => funcTransfer(50, "ole")));
            Thread thread3 = new Thread((() => funcTransfer(50, "ola")));
            Thread thread5 = new Thread((() => funcTransfer(50, "oli")));
            Thread thread6 = new Thread((() => funcTransfer(50, "olo")));
            Thread thread7 = new Thread((() => funcTransfer(50, "olu")));
            Thread thread8 = new Thread((() => funcTransfer(50, "olaq")));

            Thread thread4 = new Thread(funcTake);
            Thread t9 = new Thread(funcTake);
            Thread t10 = new Thread(funcTake);
            Thread t11 = new Thread(funcTake);
            Thread t12 = new Thread(funcTake);
            Thread t13 = new Thread(funcTake);


            thread2.Start();
            t9.Start();
            thread3.Start();

            thread4.Start();
            thread5.Start();

            t10.Start();
            t11.Start();

            thread6.Start();
            thread7.Start();
            thread8.Start();


            t12.Start();
            t13.Start();


            if (queue.Count == 6)
            {
                Console.WriteLine("TEST SUCCESSFULY FINNISHED");
                return true;
            }

            return false;
        }


        static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();


        private static void funcTake()
        {
            string rmsg;
            bool ret;
            try
            {
                if (ret = _queue.Take(50, out rmsg))
                {
                    queue.Enqueue(rmsg);
                    Console.WriteLine(rmsg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void funcTransfer(int timeout, string message)
        {
            try
            {
                bool isTaken = _queue.Transfer(message, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
