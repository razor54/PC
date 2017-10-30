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
        private Pairing2<int, string> pairing = new Pairing2<int, string>();

        ConcurrentQueue<Tuple<int, string>> queue = new ConcurrentQueue<Tuple<int, string>>();

       


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
        public void testPairing()
        {
            queue = new ConcurrentQueue<Tuple<int, string>>();
            pairing = new Pairing2<int, string>();
            int MAX = 50;


           
            Thread f4th = new Thread(()=>funGen("tres",1000));
            Thread f5th = new Thread(()=>funGen("quatro",1000));
            Thread f6th = new Thread(()=>funGen("nove",1000));

            Thread f7th = new Thread(() => funGenT(3, 1000));
            Thread f8th = new Thread(() => funGenT(4, 1000));
            Thread f9th = new Thread(() => funGenT(5, 1000));

           

            f4th.Start();
            f5th.Start();
            f6th.Start();

            f7th.Start();
            f8th.Start();
            f9th.Start();


            f4th.Join();
            f5th.Join();
            f6th.Join();

            f7th.Join();
            f8th.Join();
            f9th.Join();

            Assert.AreEqual(6, queue.Count);

            var array = queue.ToArray();

            for (int i = 0; i < array.Length; i+=2)
            {
                var tuple = array[i];
                
                Assert.AreEqual(2, array.Count(e => e.Equals(tuple)));
            }

        }


    }
}
