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

        private static readonly ExpirableVal<T> Busy = new ExpirableVal<T>(default(TimeSpan), null);
        private static ExpirableVal<T> _currval;

        public ExpirableLazyOptimized(Func<T> provider, TimeSpan timeToLive)
        {
            _provider = provider;
            _timeToLive = timeToLive;
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
                            continue;


                        currValue = GetCurrValue();
                        if (currValue != null)
                            return currValue;

                        Recalculate();
                        return _currval.Value;
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
            while (true)
            {
                var curr = Interlocked.CompareExchange(ref _currval, _currval, Busy);

                if (curr != null && curr != Busy && !IsTimeOut())
                {
                    return curr.Value;
                }

                if (curr == Busy) continue;

                Recalculate();
                if (_currval != null)
                    return _currval.Value;
            }
        }

        private void Recalculate()
        {
            //???
            if (Interlocked.Exchange(ref _currval, Busy) == Busy) return;


            ExpirableVal<T> newVal;
            try
            {
                newVal = new ExpirableVal<T>(GetNewExpiration(), _provider());
            }

            catch (Exception)
            {
                Interlocked.Exchange(ref _currval, null);

                return;
            }

            Interlocked.CompareExchange(ref _currval, newVal, Busy);
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
