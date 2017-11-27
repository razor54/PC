using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExpirableLazyOptimized
{
    public class ExpirableLazyOptimized<T> where T : class
    {
        private readonly Func<T> _provider;
        private readonly TimeSpan _timeToLive;
        private readonly object _mon = new object();
        private static readonly ExpirableVal<T> Busy = new ExpirableVal<T>(default(TimeSpan), null);
        private static ExpirableVal<T> _currval;

        private readonly int _maxTimeout;
        public ExpirableLazyOptimized(Func<T> provider, TimeSpan timeToLive)
        {
            _provider = provider;
            _timeToLive = timeToLive;
            //rounds the total time and casts to int
            _maxTimeout = Convert.ToInt32(timeToLive.TotalMilliseconds);
        }

        public T Value
        {
            get
            {
                try
                {
                    T currValue;

                    while (true)
                    {
                        currValue = GetCurrValue();

                        if (currValue != null)
                            return currValue;

                        if (Interlocked.CompareExchange(ref _currval, _currval, Busy) == Busy)
                        {
                            Monitor.Enter(_mon);
                            bool suceed = Monitor.Wait(_mon,_maxTimeout);
                            Monitor.Exit(_mon);
                            if (!suceed)continue;

                        }


                        currValue = GetCurrValue();
                        if (currValue != null)
                            return currValue;

                        Recalculate();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        } // throws InvalidOperationException, ThreadInterruptedException

        private T GetCurrValue()
        {
            var curr = Interlocked.CompareExchange(ref _currval, _currval, Busy);

            if (curr != null && curr != Busy && !IsTimeOut())
            {
                return curr.Value;
            }


            curr = Interlocked.CompareExchange(ref _currval, _currval, Busy);
            return curr != null && curr != Busy ? curr.Value : null;
        }

        private void Recalculate()
        {
            while (Interlocked.Exchange(ref _currval, Busy) == Busy)
            {
                Monitor.Enter(_mon);
                bool sucess = Monitor.Wait(_mon,_maxTimeout);
                Monitor.Exit(_mon);
                if(sucess)break;
            }


            ExpirableVal<T> newVal;
            try
            {
                newVal = new ExpirableVal<T>(GetNewExpiration(), _provider());
                Monitor.Enter(_mon);
                Monitor.PulseAll(_mon);
            }

            catch (Exception)
            {
                Interlocked.Exchange(ref _currval, null);
                Monitor.Enter(_mon);
                Monitor.Pulse(_mon);
                Monitor.Exit(_mon);
                return;
            }

            Interlocked.CompareExchange(ref _currval, newVal, Busy);
            Monitor.Exit(_mon);
        }

        private Boolean IsTimeOut()
        {
            if (_currval == null) return false;

            return DateTime.Now.TimeOfDay.Ticks >= _currval.ValidUntil.Ticks;
        }

        private TimeSpan GetNewExpiration()
        {
            return _timeToLive.Add(DateTime.Now.TimeOfDay);
        }
    }
}
