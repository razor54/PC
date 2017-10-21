using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Pairing
{
    class PairingTest
    {
        private Pairing<int, string> pairing = new Pairing<int, string>();

        ConcurrentQueue<Tuple<int, string>> queue = new ConcurrentQueue<Tuple<int, string>>();

        [Test]
        public void testPairing()
        {
            pairing = new Pairing<int, string>();
            int MAX = 50;

            for (int i = 1; i < MAX + 1; i++)
            {
                Thread firsThread = new Thread(fun);
                Thread second = new Thread(fun2);

                firsThread.Start();
                second.Start();

                firsThread.Join();
                second.Join();

                Assert.AreEqual(2, queue.Count);

                Assert.AreEqual(queue.First(), queue.Last());
                queue = new ConcurrentQueue<Tuple<int, string>>();
            }
        }

        public void fun()
        {
            var provide = pairing.Provide(5, 1000);
            Console.WriteLine("O Provide de 5 deu {0}", provide);
            queue.Enqueue(provide);
        }

        public void fun2()
        {
            var q = pairing.Provide("cinco", 1000);
            Console.WriteLine("O Provide de cinco deu {0}", q);
            queue.Enqueue(q);
        }

        public void funGen(string x, int y)
        {
            var q = pairing.Provide(x, y);
            Console.WriteLine("O Provide de {1} deu {0}", q, x);
            queue.Enqueue(q);
        }

        public void funGenT(int x, int y)
        {
            var q = pairing.Provide(x, y);
            Console.WriteLine("O Provide de {1} deu {0}", q, x);
            queue.Enqueue(q);
        }

        [Test]
        public void testPairing2()
        {
            pairing = new Pairing<int, string>();
            int MAX = 50;

            Thread firsThread = new Thread(fun);
            Thread second = new Thread(fun2);

           
            Thread f4th = new Thread(()=>funGen("tres",1000));
            Thread f5th = new Thread(()=>funGen("quatro",1000));
            Thread f6th = new Thread(()=>funGen("nove",1000));

            Thread f7th = new Thread(() => funGenT(3, 1000));
            Thread f8th = new Thread(() => funGenT(4, 1000));
            Thread f9th = new Thread(() => funGenT(5, 1000));

            firsThread.Start();
            second.Start();

            f4th.Start();
            f5th.Start();
            f6th.Start();

            f7th.Start();
            f8th.Start();
            f9th.Start();

            firsThread.Join();
            second.Join();

            f4th.Join();
            f5th.Join();
            f6th.Join();

            f7th.Join();
            f8th.Join();
            f9th.Join();

            Assert.AreEqual(8, queue.Count);

            var array = queue.ToArray();

            for (int i = 0; i < array.Length; i+=2)
            {
                Assert.AreEqual(array[i], array[i + 1]);
            }

        }



        [Test]
        public void solamente()
        {
            object obj = new object();
            Thread t1 = new Thread(() =>
            {
                Monitor.Enter(obj);
                Monitor.Wait(obj);
                Console.WriteLine("T1 is done");
                Monitor.Exit(obj);
                
            });
            Thread t2 = new Thread(() =>
            {
                Monitor.Enter(obj);
                
                Monitor.Pulse(obj);
                
                Console.WriteLine("T2 is done");

                Monitor.Exit(obj);
            });

            t2.Start();
           
            t1.Start();
            t1.Join();
            t2.Join();


        }

    }
}
