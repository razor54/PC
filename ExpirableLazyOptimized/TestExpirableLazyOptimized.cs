using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ExpirableLazyOptimized
{
    class TestExpirableLazyOptimized
    {
        class TestExpirableLazy
        {
            private ExpirableLazyOptimized<String> expirable = new ExpirableLazyOptimized<String>(
                () => 4.ToString() + "--" + Thread.CurrentThread.ManagedThreadId.ToString() + "---" +
                      DateTime.Now.TimeOfDay,
                new TimeSpan(0, 0, 0, 1)
            );

            [Test]
            public void Test1()
            {
                Thread firtsThread = new Thread(fun);
                Thread secondThread = new Thread(fun);
                Thread thirdThread = new Thread(fun);
                firtsThread.Start();
                
                secondThread.Start();
               
                thirdThread.Start();

                firtsThread.Join();
                secondThread.Join();
                thirdThread.Join();
              

                Assert.AreEqual(3, _results3.Count);

                Assert.AreEqual(true,_results3.All(e=>e!=null));
            }

            private readonly ConcurrentQueue<string> _results = new ConcurrentQueue<string>();
            private readonly ConcurrentQueue<string> _results2 = new ConcurrentQueue<string>();
            private readonly ConcurrentQueue<string> _results3 = new ConcurrentQueue<string>();

            private void fun1()
            {
                Thread.Sleep(500);
                var expirableValue = expirable.Value;
                //Waiting time expires 
                _results.Enqueue(expirableValue);
                Console.WriteLine(expirableValue);
            }

            private void fun()
            {
                Thread.Sleep(500);
                var expirableValue = expirable.Value;
                //Waiting time expires 
                _results3.Enqueue(expirableValue);
                Console.WriteLine(expirableValue);
            }

            [Test]
            public void Test2()
            {
                Thread firtsThread = new Thread(fun1);
                Thread secondThread = new Thread(fun1);
                Thread thirdThread = new Thread(fun1);
                Thread thread4 = new Thread(fun1);
                Thread thread5 = new Thread(fun1);
                Thread thread6 = new Thread(fun1);
                Thread thread7 = new Thread(fun1);
                Thread thread8 = new Thread(fun1);
                Thread thread9 = new Thread(fun1);
                Thread thread10 = new Thread(fun1);

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

                firtsThread.Join();
                secondThread.Join();
                secondThread.Join();
                thread4.Join();
                thread5.Join();
                thread6.Join();
                thread7.Join();
                thread8.Join();
                thread9.Join();
                thread10.Join();

                Assert.AreEqual(10, _results.Count);
                Assert.AreEqual(true, _results3.All(e => e != null));
            }


            [Test]
            public void Test3()
            {
                Thread firtsThread = new Thread(fun2);
                Thread secondThread = new Thread(fun2);
                Thread thirdThread = new Thread(fun2);
                Thread thread4 = new Thread(fun2);
                Thread thread5 = new Thread(fun2);
                Thread thread6 = new Thread(fun2);
                Thread thread7 = new Thread(fun2);
                Thread thread8 = new Thread(fun2);
                Thread thread9 = new Thread(fun2);
                Thread thread10 = new Thread(fun2);

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

                Thread.Sleep(3000);
                Assert.AreEqual(10, _results2.Count);
                Assert.AreEqual(true, _results3.All(e => e != null));
            }

            private void fun2()
            {
                Thread.Sleep(500);
                var expirableValue = expirable.Value;
                //Waiting time expires 
                _results2.Enqueue(expirableValue);
                Console.WriteLine(expirableValue);
            }
        }
    }
}
