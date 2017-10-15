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
    class TestTransferQueue
    {
        private readonly TransferQueue<string> _queue = new TransferQueue<string>();

        [Test]
        public void test1()
        {
            queue = new ConcurrentQueue<string>();
            Thread thread1 = new Thread((() => _queue.Transfer("ola", 5)));

            Thread thread2 = new Thread((() => _queue.Transfer("ole", 5)));

            Thread thread3 = new Thread(funcTake);
            Thread thread4 = new Thread(funcTake);

            thread1.Start();
            thread2.Start();
           
            thread4.Start();
            thread3.Start();

            Thread.Sleep(100);
            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void test2()
        {
            
            queue=new ConcurrentQueue<string>();
            Thread thread3 = new Thread(funcTake);
            Thread thread4 = new Thread(funcTake);

            Thread thread1 = new Thread((() => _queue.Transfer("ola", 5)));

            Thread thread2 = new Thread((() => _queue.Transfer("ole", 5)));

            thread3.Start();
            thread4.Start();

            thread1.Start();
            thread2.Start();
           

            Thread.Sleep(100);
            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void test3()
        {

            queue = new ConcurrentQueue<string>();
            Thread thread1 = new Thread(funcTake);
            Thread thread2 = new Thread((() => _queue.Transfer("ola", 5)));
            Thread thread3 = new Thread(funcTake);
            Thread thread4 = new Thread((() => _queue.Transfer("ole", 5)));

            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();

            Thread.Sleep(100);

            Assert.AreEqual(2,queue.Count);
        }


        [Test]
        public void test4()
        {

            queue = new ConcurrentQueue<string>();

            Thread thread3 = new Thread(funcTake);
           
            Thread thread4 = new Thread(funcTake);

            Thread thread2 = new Thread((() => funcTransfer(5, "ole")));

           
           
            thread3.Start();
            thread4.Start();
            thread2.Start();

            Thread.Sleep(100);

            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void test5()
        {

            queue = new ConcurrentQueue<string>();

           
            Thread thread4 = new Thread(funcTake);

            Thread thread2 = new Thread((() => funcTransfer(5, "ole")));

            Thread thread3 = new Thread((() => funcTransfer(5, "ola")));

            thread4.Start();
            Thread.Sleep(1);
            thread2.Start();
            thread3.Start();
            

            Thread.Sleep(100);

            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void test6()
        {

            queue = new ConcurrentQueue<string>();


            

            Thread thread2 = new Thread((() => funcTransfer(5, "ole")));

            Thread thread3 = new Thread((() => funcTransfer(5, "ola")));

            Thread thread4 = new Thread(funcTake);

            thread2.Start();
            thread3.Start();
            thread4.Start();

            Thread.Sleep(100);

            Assert.AreEqual(1, queue.Count);
        }


        [Test]
        public void test7()
        {

            queue = new ConcurrentQueue<string>();

            Thread thread2 = new Thread((() => funcTransfer(5, "ole")));

            Thread thread3 = new Thread((() => funcTransfer(5, "ola")));
            Thread thread5 = new Thread((() => funcTransfer(5, "oli")));
            Thread thread6 = new Thread((() => funcTransfer(5, "olo")));
            Thread thread7 = new Thread((() => funcTransfer(5, "olu")));
            Thread thread8 = new Thread((() => funcTransfer(5, "olaq")));

            Thread thread4 = new Thread(funcTake);
            Thread t9 = new Thread(funcTake);
            Thread t10 = new Thread(funcTake);
            Thread t11 = new Thread(funcTake);


            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();
            thread6.Start();
            thread7.Start();
            thread8.Start();

            t9.Start();
            t10.Start();
            t11.Start();

            Thread.Sleep(200);

            Assert.AreEqual(4, queue.Count);
        }



        [Test]
        public void test8()
        {

            queue = new ConcurrentQueue<string>();

            Thread thread2 = new Thread((() => funcTransfer(10, "ole")));
            Thread thread3 = new Thread((() => funcTransfer(10, "ola")));
            Thread thread5 = new Thread((() => funcTransfer(10, "oli")));
            Thread thread6 = new Thread((() => funcTransfer(10, "olo")));
            Thread thread7 = new Thread((() => funcTransfer(10, "olu")));
            Thread thread8 = new Thread((() => funcTransfer(10, "olaq")));

            Thread thread4 = new Thread(funcTake);
            Thread t9 = new Thread(funcTake);
            Thread t10 = new Thread(funcTake);
            Thread t11 = new Thread(funcTake);
            Thread t12 = new Thread(funcTake);
            Thread t13 = new Thread(funcTake);
           

            thread2.Start();
            
            thread3.Start();

            thread4.Start();
            thread5.Start();
            t9.Start();
            t10.Start();
            t11.Start();

            thread6.Start();
            thread7.Start();
            thread8.Start();
            
            t12.Start();
            t13.Start();
            

            Thread.Sleep(200);

            Assert.AreEqual(6, queue.Count);
        }


        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

        
        private void funcTake()
        {
            string rmsg;
            bool ret;
            try
            {
                if (ret = _queue.Take(10, out rmsg))
                {
                    queue.Enqueue(rmsg);
                    Console.WriteLine(rmsg);
                }
                else
                {
                    Console.WriteLine("HAS NOT RECEIVE A MESSAGE");
                }
            }
            catch (Exception e )
            {
                Console.WriteLine(e );
            }
        }

        private void funcTransfer(int timeout,string message)
        {
            try
            {
                bool isTaken = _queue.Transfer(message, timeout);
                if (!isTaken)
                {
                    Console.WriteLine("THE MESSAGE WAS NOT TAKEN");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        
    }
}
