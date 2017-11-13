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
            Interlocked.MemoryBarrier();
            if (_refCount == 0)
                throw new InvalidOperationException();
            Interlocked.Increment(ref _refCount);
        }

        public void ReleaseRef()
        {
            Interlocked.MemoryBarrier();
            if (_refCount == 0)
                throw new InvalidOperationException();

            if (Interlocked.Decrement(ref _refCount) != 0) return;

            IDisposable disposable = _value as IDisposable;
            _value = null;
            disposable?.Dispose();
        }

        public T Value
        {
            get
            {
                Interlocked.MemoryBarrier();
                if (_refCount == 0)
                    throw new InvalidOperationException();
                return _value;
            }
        }
    }
}
