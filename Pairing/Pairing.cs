using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyncUtils;
using SyncUtils = SyncUtils.SyncUtils;

namespace Pairing
{
    public class Pairing<T, U>
    {
        private readonly LinkedList<T> _tList = new LinkedList<T>();
        private readonly LinkedList<U> _uList = new LinkedList<U>();
        private readonly object _monitor = new object();

        private T _element1;
        private U _element2;

        private Tuple<T, U> tuple;

        // throws ThreadInterruptedException, TimeoutException
        public Tuple<T, U> Provide(T value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);
                var last = _tList.AddLast(value);
                if (last == _tList.First && !timeoutInstant.IsTimeout)
                {
                    return TupleGetter(value, timeoutInstant, last);
                }

                if (timeoutInstant.IsTimeout) return Failure(last);


                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                //you have to be first
                return TupleGetter(value, timeoutInstant, last);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                throw e;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        private Tuple<T, U> Failure<TX>(LinkedListNode<TX> last)
        {
            _element1 = default(T);
            _element2 = default(U);

            Console.WriteLine("deu null para  {0} no T", last.Value);
            return tuple = null;
        }

        private Tuple<T, U> TupleGetter(T value, TimeoutInstant timeoutInstant, LinkedListNode<T> last)
        {
            _element1 = value;

            if (timeoutInstant.IsTimeout) return tuple = null;

            if (_uList.Any())
            {

                if (_tList.Count == 1)
                {
                    Monitor.Pulse(_monitor);
                }
                else
                    global::SyncUtils.SyncUtils.Wait(_monitor, _uList, timeoutInstant.Remaining);


                 tuple = new Tuple<T, U>(_element1, _element2);


                global::SyncUtils.SyncUtils.Pulse(_monitor, _tList);

                _tList.Remove(last);
                

                if (_tList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _tList.First);

                return tuple;
            }
           
            //esperar para poder continuar
            Monitor.Wait(_monitor,timeoutInstant.Remaining);


            tuple = new Tuple<T, U>(_element1, _element2);


            global::SyncUtils.SyncUtils.Pulse(_monitor, _tList);

            _tList.Remove(last);


            if (_tList.Any())
                global::SyncUtils.SyncUtils.Pulse(_monitor, _tList.First);

            return tuple;



        }


        public Tuple<T, U> Provide(U value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);

                var last = _uList.AddLast(value);
                //FIFO ORDER
                if (last == _uList.First && !timeoutInstant.IsTimeout)
                {
                    return TupleGetter(value, timeoutInstant, last);
                }

                Console.WriteLine("Not first no U ");

                if (timeoutInstant.IsTimeout) return Failure(last);


                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);


                return TupleGetter(value, timeoutInstant, last);
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                throw;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        private Tuple<T, U> TupleGetter(U value, TimeoutInstant timeoutInstant, LinkedListNode<U> last)
        {
            _element2 = value;
            if (_tList.Any())
            {
                //Nao tinha ninguem antes na fila e por isso o t viu que nao havia ninguem nesta lista de u
                if (_uList.Count == 1)
                {
                    Monitor.Pulse(_monitor);
                    
                }
                else
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uList);

                global::SyncUtils.SyncUtils.Wait(_monitor, _tList, timeoutInstant.Remaining);


                _uList.Remove(last);

                if (_uList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uList.First);

                if (tuple == null) Console.WriteLine("Tuple null");
                return tuple;
            }

            Monitor.Wait(_monitor,timeoutInstant.Remaining);

            _uList.Remove(last);

            if (_uList.Any())
                global::SyncUtils.SyncUtils.Pulse(_monitor, _uList.First);

            return tuple;
        }
    }
}
