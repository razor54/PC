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


        // throws ThreadInterruptedException, TimeoutException
        public Tuple<T, U> Provide(T value, int timeout)
        {
            Monitor.Enter(_monitor);

            try
            {
                TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);
                var last = _tList.AddLast(value);
                T curr = last.Value;
                Tuple<T, U> tuple;
                if (_uList.Any())
                {
                    var uListFirst = _uList.First;
                    global::SyncUtils.SyncUtils.Pulse(_monitor, uListFirst);
                    _uList.RemoveFirst();
                    _tList.Remove(last);
                    tuple = new Tuple<T, U>(curr, uListFirst.Value);
                    _tuples.AddLast(tuple);

                    if (timeoutInstant.IsTimeout) return null;

                    global::SyncUtils.SyncUtils.Wait(_monitor, tuple, timeoutInstant.Remaining);
                    return tuple;
                }
                if (timeoutInstant.IsTimeout) return null;

                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);
               

                tuple = _tuples.First(t => t.Item1.Equals(curr));
                global::SyncUtils.SyncUtils.Pulse(_monitor, tuple);
                return tuple;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                return null;
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
                Tuple<T, U> tuple;

                if (_tList.Any())
                {
                    var tListFirst = _tList.First;
                    global::SyncUtils.SyncUtils.Pulse(_monitor, tListFirst);
                    _tList.RemoveFirst();
                    _uList.Remove(last);
                    tuple = new Tuple<T, U>(tListFirst.Value, curr);
                    _tuples.AddLast(tuple);
                    if (timeoutInstant.IsTimeout) return null;
                    global::SyncUtils.SyncUtils.Wait(_monitor, tuple, timeoutInstant.Remaining);

                    return tuple;
                }

               
                if (timeoutInstant.IsTimeout) return null;
                global::SyncUtils.SyncUtils.Wait(_monitor, last, timeoutInstant.Remaining);

                

                 tuple = _tuples.First(t => t.Item2.Equals(curr));
                global::SyncUtils.SyncUtils.Pulse(_monitor, tuple);
                return tuple;
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Exceçao");
                return null;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
    }
}
