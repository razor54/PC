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
                LinkedListNode<U> uListFirst;

                if (node == _tList.First)
                {
                    _elemt1 = node.Value;

                    

                    Monitor.Enter(_tList);
                    Monitor.Pulse(_tList);
                    Monitor.Exit(_tList);

                    if ((uListFirst = _uList.First) != null)
                    {
                        Monitor.Enter(uListFirst);
                        Monitor.Pulse(uListFirst);
                        Monitor.Exit(uListFirst);
                    }
                     Monitor.Exit(_monitor);
                    Monitor.Enter(_uList);
                   

                    bool isNotTimeout = Monitor.Wait(_uList, timeout);

                    Monitor.Exit(_uList);
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
                    if (_elemt1 == null || _elemt1.Equals(default(T)) || _element2 == null ||
                        _element2.Equals(default(U)))
                    {
                        toRet = null;
                        //threre was something wrong so return null
                        return null;
                    }
                    //retirar os valores para a proxima iteraçao
                    Console.WriteLine("SAIU PPOR PROVIDE T -- 1");
                    toRet = new Tuple<T, U>(_elemt1, _element2);
                    return toRet;
                }
                Monitor.Exit(_monitor);


                Monitor.Enter(_uList);

                bool isnotTimeout = Monitor.Wait(_uList, timeout);
                Monitor.Exit(_uList);

                Monitor.Enter(node);
                Monitor.Wait(node, timeout);
                Monitor.Exit(node);
                Monitor.Enter(_monitor);

                if (!isnotTimeout)
                {
                    //tirar os pairings
                    _elemt1 = default(T);
                    _element2 = default(U);

                    return null;
                }
                Monitor.Exit(_monitor);

                Monitor.Enter(_tList);
                Monitor.Pulse(_tList);
                Monitor.Exit(_tList);

                uListFirst = _uList.First;

                Monitor.Enter(uListFirst);
                Monitor.Pulse(uListFirst);
                Monitor.Exit(uListFirst);

                Monitor.Enter(_monitor);
                Console.WriteLine("SAIU PPOR PROVIDE T---2");
                return new Tuple<T, U>(_elemt1, _element2);
                /*
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
                uListFirst = _uList.First;
                //retirar os valores para a proxima iteraçao
                Monitor.Enter(uListFirst);
                Monitor.Pulse(uListFirst);
                Monitor.Exit(uListFirst);
                toRet = new Tuple<T, U>(_elemt1, _element2);
                return toRet;
                */
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
                if (_tList.Any())
                {
                    Monitor.Enter(_tList.First);
                    Monitor.Pulse(_tList.First);
                    Monitor.Exit(_tList.First);
                }
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
                LinkedListNode<T> tListFirst ;
                if (node == _uList.First)
                {
                    _element2 = node.Value;

                   

                    Monitor.Enter(_uList);
                    Monitor.Pulse(_uList);
                    Monitor.Exit(_uList);

                    if ((tListFirst = _tList.First) != null)
                    {
                        Monitor.Enter(tListFirst);
                        Monitor.Pulse(tListFirst);
                        Monitor.Exit(tListFirst);
                    }

                    Monitor.Exit(_monitor);
                    Monitor.Enter(_tList);


                    bool isNotTimeout = Monitor.Wait(_tList, timeout);
                    Monitor.Exit(_tList);
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
                    if (_elemt1 == null || _elemt1.Equals(default(T)) || _element2 == null ||
                        _element2.Equals(default(U)))
                    {
                        //threre was something wrong so return null
                        toRet = null;
                        return null;
                    }
                    toRet = new Tuple<T, U>(_elemt1, _element2);
                    ;
                    //retirar os valores para a proxima iteraçao
                    Console.WriteLine("SAIU PPOR PROVIDE U---1");
                    return toRet;
                }

                Monitor.Exit(_monitor);


                Monitor.Enter(_tList);

                bool isnotTimeout = Monitor.Wait(_tList, timeout);
                Monitor.Exit(_tList);

                Monitor.Enter(node);
                Monitor.Wait(node, timeout);
                Monitor.Exit(node);

                Monitor.Enter(_monitor);
                if (!isnotTimeout)
                {
                    //tirar os pairings
                    _elemt1 = default(T);
                    _element2 = default(U);

                    return null;
                }
                tListFirst = _tList.First;
                Monitor.Exit(_monitor);


                Monitor.Enter(_uList);
                Monitor.Pulse(_uList);
                Monitor.Exit(_uList);

                Monitor.Enter(tListFirst);
                Monitor.Pulse(tListFirst);
                Monitor.Exit(tListFirst);

                Monitor.Enter(_monitor);
                Console.WriteLine("SAIU PPOR PROVIDE U---2");
                return new Tuple<T, U>(_elemt1, _element2);
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
                if (_uList.Any())
                {
                    Monitor.Enter(_uList.First);
                    Monitor.Pulse(_uList.First);
                    Monitor.Exit(_uList.First);
                }
                Monitor.Exit(_monitor);
            }
        }
    }
}
