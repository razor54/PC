using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefCountedHolder
{
    class RefCounterHolderTest
    {
        [Test]
        public void test1()
        {
            string s = "ola";
            int UPPER = 20;
            RefCountedHolder<string> refCountedHolder = new RefCountedHolder<string>(s);

            for (int i = 0; i < UPPER; i++)
            {
               

                Thread t1 = new Thread(t => { refCountedHolder.AddRef(); });
                Thread t2 = new Thread(t => { refCountedHolder.AddRef(); });

                Thread t3 = new Thread(t =>
                {
                    refCountedHolder.ReleaseRef();
                    refCountedHolder.ReleaseRef();
                });

                t1.Start();
                t2.Start();

                t1.Join();
                t2.Join();

                t3.Start();

                t3.Join();

                Assert.AreEqual(refCountedHolder.Value, s);
                
            }
            Assert.AreEqual(s,refCountedHolder.Value);
            refCountedHolder.ReleaseRef();

            Assert.Throws<InvalidOperationException>(()=>
            {
                var value = refCountedHolder.Value;      
            });



        }

        [Test]
        public void testInterlockedExchange()
        {
            
            int res = 2;
            int valToStore = 5;
            int s = 2;

            int x = Interlocked.CompareExchange(ref res, valToStore, s);
            Assert.AreEqual(s,x);
        }

    }
}
