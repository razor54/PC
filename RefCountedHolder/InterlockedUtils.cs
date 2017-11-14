using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefCountedHolder
{
    static class InterlockedUtils
    {
        public static bool InterlockedCompareExchange<T>(ref T res, T val, T comparand) where T:class 
        {
            return comparand == Interlocked.CompareExchange(ref res, val, comparand);
        }
    }
}
