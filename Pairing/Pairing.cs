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

        private readonly LinkedList<object> _uListCondiion = new LinkedList<object>();
        private readonly LinkedList<object> _tListCondiion = new LinkedList<object>();

        private readonly object _monitor = new object();


        private Tuple<T, U> _tuple;

        // throws ThreadInterruptedException, TimeoutException
        public Tuple<T, U> Provide(T value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);
                var last = _tList.AddLast(value);
                var cond = _tListCondiion.AddLast(value);

                if (last == _tList.First && !timeoutInstant.IsTimeout)
                {
                    return TupleGetter(value, timeoutInstant, last, cond);
                }

                if (timeoutInstant.IsTimeout) return Failure();
                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                Console.WriteLine(last == _tList.First);
                //you have to be first
                return TupleGetter(value, timeoutInstant, last, cond);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                throw e;
            }
            finally
            {
                if (_tList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _tList.First);

               // _tListCondiion.RemoveFirst();
                Monitor.Exit(_monitor);
            }
        }

        private Tuple<T, U> Failure()
        {
            Console.WriteLine("Failure");
            return _tuple = null;
        }

        private Tuple<T, U> TupleGetter(T value, TimeoutInstant timeoutInstant, LinkedListNode<T> last,
            LinkedListNode<object> cond)
        {
            if (timeoutInstant.IsTimeout) return Failure();

            if (_uList.Any())
            {
                if (_tList.Count == 1)
                {
                    //remover o primeiro aqui para evitar dupla notifiaçao e perda 
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uListCondiion.First);
                    //_uListCondiion.RemoveFirst();
                }
                else
                {
                    if (timeoutInstant.IsTimeout) return Failure();
                    global::SyncUtils.SyncUtils.Wait(_monitor, _uList, timeoutInstant.Remaining);
                }


                _tuple = new Tuple<T, U>(_tList.First.Value, _uList.First.Value);


                global::SyncUtils.SyncUtils.Pulse(_monitor, _tList);

                _tList.Remove(last);
                _tListCondiion.RemoveFirst();


                if (_tList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _tList.First);

                return _tuple;
            }


            if (timeoutInstant.IsTimeout) return Failure();
            //esperar para poder continuar
            global::SyncUtils.SyncUtils.Wait(_monitor, cond, timeoutInstant.Remaining);


            _tuple = new Tuple<T, U>(_tList.First.Value, _uList.First.Value);


            global::SyncUtils.SyncUtils.Pulse(_monitor, _tList);

            _tList.Remove(last);
            _tListCondiion.RemoveFirst();

            return _tuple;
        }


        public Tuple<T, U> Provide(U value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);

                var last = _uList.AddLast(value);

                var cond = _uListCondiion.AddLast(value);

                //FIFO ORDER
                if (last == _uList.First && !timeoutInstant.IsTimeout)
                {
                    return TupleGetter(value, timeoutInstant, last, cond);
                }

                Console.WriteLine("Not first no U ");


                if (timeoutInstant.IsTimeout) return Failure();

                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                Console.WriteLine(last == _uList.First);
                return TupleGetter(value, timeoutInstant, last, cond);
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                throw;
            }
            finally
            {
                if (_uList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uList.First);

                _tuple = null;
                //_uListCondiion.RemoveFirst();
                Monitor.Exit(_monitor);
            }
        }

        private Tuple<T, U> TupleGetter(U value, TimeoutInstant timeoutInstant, LinkedListNode<U> last,
            LinkedListNode<object> cond)
        {
            if (_tList.Any())
            {
                //Nao tinha ninguem antes na fila e por isso o t viu que nao havia ninguem nesta lista de u
                // global::SyncUtils.SyncUtils.Pulse(_monitor, _uList.Count == 1 &&_tListCondiion.Any() ? (object) _tListCondiion.First : _uList);

                if (_uList.Count == 1)
                {
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _tListCondiion.First);
                    // _uListCondiion.RemoveFirst();
                }
                else
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uList);

                if (timeoutInstant.IsTimeout == false)
                    global::SyncUtils.SyncUtils.Wait(_monitor, _tList, timeoutInstant.Remaining);


                _uList.Remove(last);
                _uListCondiion.RemoveFirst();

                if (_uList.Any())
                    global::SyncUtils.SyncUtils.Pulse(_monitor, _uList.First);

                return _tuple;
            }


            if (timeoutInstant.IsTimeout) return Failure();

            global::SyncUtils.SyncUtils.Wait(_monitor, cond, timeoutInstant.Remaining);

            _uList.Remove(last);
            _uListCondiion.RemoveFirst();

            return _tuple;
        }
    }
}
