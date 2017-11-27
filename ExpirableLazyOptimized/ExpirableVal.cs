using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpirableLazyOptimized
{
    class ExpirableVal<T>
    {
        public TimeSpan ValidUntil { get; }
        public T Value { get; }

        public ExpirableVal(TimeSpan validUntil, T value)
        {
            Value = value;
            ValidUntil = validUntil;
        }

    }
}
