using System;

namespace Pyrrha
{
    public class ModifiedEventArgs<T> : EventArgs
    {
        public T ValueBefore;
        public T ValueAfter;
    }
}
