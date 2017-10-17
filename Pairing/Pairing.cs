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
            Tuple<T, U> toRet = null;
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
                        toRet = null;
                        return null;
                    }

                    //verify if elems are null or not
                    //if they are it means there was a timeout or exception
                    //???
                    if (_elemt1.Equals(default(T)) || _element2.Equals(default(U)))
                    {
                        toRet = null;
                        //threre was something wrong so return null
                        return null;
                    }
                    //retirar os valores para a proxima iteraçao
                    toRet = new Tuple<T, U>(_elemt1, _element2);
                    return toRet;
                }



                Monitor.Enter(node);

                Monitor.Exit(_monitor);
                bool isNotTimeouts = Monitor.Wait(node, timeout);
                Monitor.Enter(_monitor);

                if (!isNotTimeouts)
                {
                    //tirar os pairings
                    _elemt1 = default(T);
                    _element2 = default(U);
                    toRet = null;
                    return null;
                }

                //verify if elems are null or not
                //if they are it means there was a timeout or exception
                //???
                if (_elemt1.Equals(default(T)) || _element2.Equals(default(U)))
                {
                    toRet = null;
                    //threre was something wrong so return null
                    return null;
                }
                //retirar os valores para a proxima iteraçao
                toRet = new Tuple<T, U>(_elemt1, _element2);
                return toRet;


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
                if(toRet!=null)
                     Monitor.Exit(node);
                Monitor.Exit(_monitor);
            }
        }


        /*
         * trocar os valores elem1 e element2 para um objeto que faça a validaçao das threads acessoras
         * ou seja uma thread que nao pertença ao par nao pode aceder aquele valor.
         * 
         */

        public Tuple<T, U> Provide(U value, int timeout)
        {
            int targetTime = timeout + Environment.TickCount;

            Monitor.Enter(_monitor);
            LinkedListNode<U> node = _uList.AddLast(value);

            Tuple<T, U> toRet = null;
            try
            {
                if (node == _uList.First && _tList.Any())
                {
                    LinkedListNode<T> tListFirst = _tList.First;
                    _element2 = node.Value;

                    Monitor.Enter(tListFirst);
                    Monitor.Pulse(tListFirst);
                    Monitor.Exit(tListFirst);

                    Monitor.Enter(node);

                    Monitor.Exit(_monitor);
                    bool isNotTimeout = Monitor.Wait(node, timeout);
                    Monitor.Enter(_monitor);

                    if (!isNotTimeout)
                    {
                        //tirar os pairings
                        _elemt1 = default(T);
                        _element2 = default(U);
                        toRet = null;
                        return null;
                    }

                    //verify if elems are null or not
                    //if they are it means there was a timeout or exception
                    //???
                    if (_elemt1.Equals(default(T)) || _element2.Equals(default(U)))
                    {
                        //threre was something wrong so return null
                        toRet = null;
                        return null;
                    }
                    toRet = new Tuple<T, U>(_elemt1, _element2); ;
                    //retirar os valores para a proxima iteraçao
                    return toRet;
                }
                Monitor.Exit(_monitor);

                
                while (!_tList.Any())
                {
                    if (Environment.TickCount > targetTime)
                        return null;
                }

                Monitor.Enter(_monitor);

                var first = _tList.First;
                Monitor.Enter(first);
                Monitor.Pulse(first);
                Monitor.Exit(first);

                if (node == _uList.First)
                {
                    LinkedListNode<T> tListFirst = _tList.First;
                    _element2 = node.Value;

                    Monitor.Enter(tListFirst);
                    Monitor.Pulse(tListFirst);
                    Monitor.Exit(tListFirst);

                    Monitor.Enter(node);

                    Monitor.Exit(_monitor);
                    bool isNotTimeout = Monitor.Wait(node, timeout);
                    Monitor.Enter(_monitor);

                    if (!isNotTimeout)
                    {
                        //tirar os pairings
                        _elemt1 = default(T);
                        _element2 = default(U);
                        toRet = null;
                        return null;
                    }

                    //verify if elems are null or not
                    //if they are it means there was a timeout or exception
                    //???
                    if (_elemt1.Equals(default(T)) || _element2.Equals(default(U)))
                    {
                        //threre was something wrong so return null
                        toRet = null;
                        return null;
                    }
                    toRet = new Tuple<T, U>(_elemt1, _element2); ;
                    //retirar os valores para a proxima iteraçao
                    return toRet;
                }

                return null;

            }
            catch (ThreadInterruptedException)
            {

                _elemt1 = default(T);
                _element2 = default(U);

                throw;
            }
            finally
            {
                _uList.Remove(node);
                if
                    (toRet != null)
                    Monitor.Exit(node);
                Monitor.Exit(_monitor);
            }

        }
    }
}
