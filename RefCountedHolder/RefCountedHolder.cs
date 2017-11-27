using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefCountedHolder
{
    public class RefCountedHolder<T> where T : class
    {
        private T _value;
        private int _refCount;

        public RefCountedHolder(T v)
        {
            _value = v;
            _refCount = 1;
        }

        public void AddRef()
        {
           

            if (_refCount == 0)
                throw new InvalidOperationException();
            Interlocked.Increment(ref _refCount);
        }

        public void ReleaseRef()
        {
            while (true)
            {
                
                int count = _refCount;
                if (count == 0)
                    throw new InvalidOperationException();
                int res;
                // the values were exchanged and the refcount > 0 
                if ((res = Interlocked.CompareExchange(ref _refCount, count - 1, count)) > 1 && res == count) return;

                if (res == count)
                {
                    IDisposable disposable = _value as IDisposable;
                    _value = null;
                    disposable?.Dispose();
                    return;
                }
            }
        }

        public T Value
        {
            get
            {
                
                if (_refCount == 0)
                    throw new InvalidOperationException();
                return _value;
            }
        }
    }
}
