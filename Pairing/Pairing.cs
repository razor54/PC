using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairing
{
    public class Pairing<T, U>
    {
        private readonly LinkedList<T> _tList = new LinkedList<T>();
        private readonly LinkedList<U> _uList = new LinkedList<U>();
        private readonly object _monitor = new object();

        private Tuple<T,U> pairTuple = new Tuple<T,U>(default(T),default(U));

        // throws ThreadInterruptedException, TimeoutException
        public Tuple<T, U> Provide(T value, int timeout)
        {
            Monitor.Enter(_monitor);
            LinkedListNode<T> node = _tList.AddLast(value);
            try
            {
                if (node == _tList.First && _uList.Any())
                {
                    LinkedListNode<U> uListFirst = _uList.First;
                    Monitor.Enter(uListFirst);
                    Monitor.Pulse(uListFirst);
                    Monitor.Exit(uListFirst);

                    Monitor.Enter(node);
                    bool isNotTimeout = Monitor.Wait(node,timeout);
                    if (!isNotTimeout)
                    {
                        //tirar os pairings
                        pairTuple.Item1=null;

                    }

                }


            }
            catch (ThreadInterruptedException)
            {

                throw;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        public Tuple<T, U> Provide(U value, int timeout)
        {
            Monitor.Enter(_monitor);
            LinkedListNode<U> node = _uList.AddLast(value);

            try
            {
                if (node == _uList.First && _tList.Any())
                {
                    LinkedListNode<T> tListFirst = _tList.First;

                }

            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
        
    }
    
}
