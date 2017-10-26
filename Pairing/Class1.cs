using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pairing
{
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
        public class Pairing3<T, U>
        {
            private readonly LinkedList<T> _tList = new LinkedList<T>();
            private readonly LinkedList<U> _uList = new LinkedList<U>();

            private readonly LinkedList<Tuple<T, U>> _tuplesList = new LinkedList<Tuple<T, U>>();


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
                    LinkedListNode<U> uListFirst;
                    if (_uList.Any())
                    {
                        uListFirst = _uList.First;
                        global::SyncUtils.SyncUtils.Pulse(_monitor, uListFirst);
                        _uList.RemoveFirst();

                        return TupleValue(last, uListFirst);

                    }
                    //  do
                    // {
                    global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);
                    //   } while (!_uList.Any());


                    uListFirst = _uList.First;
                    _uList.RemoveFirst();

                    return TupleValue(last, uListFirst);

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

                    LinkedListNode<T> tListFirst;
                    if (_tList.Any())
                    {
                        tListFirst = _tList.First;
                        global::SyncUtils.SyncUtils.Pulse(_monitor, tListFirst);
                        _tList.RemoveFirst();

                        return TupleValue(tListFirst, last);

                    }
                    // do
                    // {
                    global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                    //  } while (!_uList.Any());


                    tListFirst = _tList.First;
                    _tList.RemoveFirst();

                    return TupleValue(tListFirst, last);
                }


                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Exceçao");
                    throw;
                }
                finally
                {

                    _tuple = null;
                    Monitor.Exit(_monitor);
                }
            }

            private Tuple<T, U> TupleValue(LinkedListNode<T> tListFirst, LinkedListNode<U> last)
            {
                if (_tuplesList.Any())
                {
                    var tuple = _tuplesList.First;
                    _tuplesList.RemoveFirst();
                    return tuple.Value;
                }
                _tuple = new Tuple<T, U>(tListFirst.Value, last.Value);
                _tuplesList.AddLast(_tuple);
                return _tuple;

            }
        }
    }

}
