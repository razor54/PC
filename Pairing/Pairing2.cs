using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyncUtils;
using SyncUtils = SyncUtils.SyncUtils;

namespace Pairing
{
    public class Pairing2<T, U>
    {
        private readonly LinkedList<T> _tList = new LinkedList<T>();
        private readonly LinkedList<U> _uList = new LinkedList<U>();

        private readonly object _monitor = new object();

        private readonly LinkedList<Tuple<T, U>> _tuples = new LinkedList<Tuple<T, U>>();


        private Tuple<T, U> _tuple;

        // throws ThreadInterruptedException, TimeoutException
        public Tuple<T, U> Provide(T value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);
                var last = _tList.AddLast(value);
                LinkedListNode<U> uListFirst;
                T curr = last.Value;

                if (_uList.Any())
                {
                    uListFirst = _uList.First;
                    global::SyncUtils.SyncUtils.Pulse(_monitor, uListFirst);
                    _uList.RemoveFirst();


                    if (_tuples.Any(tuple => tuple.Item1.Equals(curr)))
                    {
                        var t = _tuples.First(tuple => tuple.Item1.Equals(curr));
                        global::SyncUtils.SyncUtils.Pulse(_monitor, t);
                        return t;
                    }

                    _tuple = new Tuple<T, U>(curr, uListFirst.Value);

                    _tuples.AddLast(_tuple);
                    global::SyncUtils.SyncUtils.Wait(_monitor, _tuple, timeoutInstant.Remaining);

                    return _tuple;
                }
                //    do
                //  {
                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);
                //    } while (!_uList.Any());


                uListFirst = _uList.First;
                _uList.RemoveFirst();


                if (_tuples.Any(tuple => tuple.Item1.Equals(curr)))
                {
                    var t = _tuples.First(tuple => tuple.Item1.Equals(curr));
                    global::SyncUtils.SyncUtils.Pulse(_monitor, t);
                    return t;
                }

                _tuple = new Tuple<T, U>(curr, uListFirst.Value);
                _tuples.AddLast(_tuple);
                global::SyncUtils.SyncUtils.Wait(_monitor, _tuple, timeoutInstant.Remaining);

                return _tuple;
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


        public Tuple<T, U> Provide(U value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);

                var last = _uList.AddLast(value);
                var curr = last.Value;


                LinkedListNode<T> tListFirst;
                if (_tList.Any())
                {
                    tListFirst = _tList.First;
                    global::SyncUtils.SyncUtils.Pulse(_monitor, tListFirst);
                    _tList.RemoveFirst();


                    if (_tuples.Any(tuple => tuple.Item2.Equals(curr)))
                    {
                        var t = _tuples.First(tuple => tuple.Item2.Equals(curr));
                        global::SyncUtils.SyncUtils.Pulse(_monitor, t);
                        return t;
                    }


                    _tuple = new Tuple<T, U>(tListFirst.Value, curr);
                    _tuples.AddLast(_tuple);
                    global::SyncUtils.SyncUtils.Wait(_monitor, _tuple, timeoutInstant.Remaining);
                    return _tuple;
                }
                // do
                // {
                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                //  } while (!_uList.Any());


                tListFirst = _tList.First;
                _tList.RemoveFirst();

                if (_tuples.Any(tuple => tuple.Item2.Equals(curr)))
                {
                    var t = _tuples.First(tuple => tuple.Item2.Equals(curr));
                    global::SyncUtils.SyncUtils.Pulse(_monitor, t);
                    return t;
                }


                _tuple = new Tuple<T, U>(tListFirst.Value, curr);
                _tuples.AddLast(_tuple);

                //retornar apenas quando os dois providers se sincronizam
                global::SyncUtils.SyncUtils.Wait(_monitor, _tuple, timeoutInstant.Remaining);

                return _tuple;
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
    }
}
