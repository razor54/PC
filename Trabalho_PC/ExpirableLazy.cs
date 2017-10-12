using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trabalho_PC
{
    public class ExpirableLazy<T> where T : class
    {
        private readonly Func<T> _provider;
        private readonly TimeSpan _timeToLive;
        private readonly object mon = new object();

        public ExpirableLazy(Func<T> provider, TimeSpan timeToLive)
        {
            _provider = provider;
            _timeToLive = timeToLive;
        }

        private T _value = null;
        private TimeSpan _target;
        private bool _isComputing = false;

        public T Value
        {
            get
            {
                {
                    try
                    {
                        Monitor.Enter(mon);
                        if (_value != null && !IsTimeOut() && !_isComputing)
                        {
                            return _value;
                        }
                        else if (_isComputing)
                        {
                            Monitor.Wait(mon);
                            if (!IsTimeOut() && _value != null) return _value;

                            Recalculate();
                            return _value;
                        }
                        else
                        {
                            Recalculate();
                            return _value;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        Monitor.Exit(mon);
                    }
                }
            }
        } // throws InvalidOperationException, ThreadInterruptedException

        private void Recalculate()
        {
            _isComputing = true;
            _value = null;

            Monitor.Exit(mon);
            try
            {
                _value = _provider();
            }

            catch (Exception)
            {
                _value = null;
                //choose a new thread to calculate
                Monitor.Pulse(mon);
            }
            
            Monitor.Enter(mon);
            _isComputing = false;
            Monitor.PulseAll(mon);
            _target = _timeToLive.Add(DateTime.Now.TimeOfDay);
        }

        private Boolean IsTimeOut()
        {
            return DateTime.Now.TimeOfDay.Ticks >= _target.Ticks;
        }
    }
}
