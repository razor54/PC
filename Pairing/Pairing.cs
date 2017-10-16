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

        private T _elemt1;
        private U _element2;

        

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

                    _elemt1 = node.Value;

                    Monitor.Enter(uListFirst);
                    Monitor.Pulse(uListFirst);
                    Monitor.Exit(uListFirst);

                    Monitor.Enter(node);

                    Monitor.Exit(_monitor);
                    bool isNotTimeout = Monitor.Wait(node, timeout);
                    Monitor.Enter(_monitor);

                    if (!isNotTimeout)
                    {
                        //tirar os pairings
                        _elemt1 = default(T);
                        _element2 = default(U);

                        return null;
                    }

                    //verify if elems are null or not
                    //if they are it means there was a timeout or exception
                    //???
                    if (_elemt1.Equals(default(T)) || _element2.Equals(default(U)))
                    {
                        //threre was something wrong so return null
                        return null;
                    }
                    //retirar os valores para a proxima iteraçao
                    return new Tuple<T, U>(_elemt1,_element2);
                }

            }
            catch (ThreadInterruptedException)
            {
                _elemt1 = default(T);
                _element2 = default(U);
                throw;
            }
            finally
            {
                _tList.Remove(node);
                Monitor.Exit(node);
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
            catch (ThreadInterruptedException)
            {
                _elemt1 = default(T);
                _element2 = default(U);

                throw;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
    }
}
