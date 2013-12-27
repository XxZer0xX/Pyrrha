#region Referenceing

using System.Collections;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Pyrrha.Collections.Enumerators
{
    public class SafeDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly IEnumerator<TKey> _keyEnumerator;
        private readonly IEnumerator<TValue> _valueEnumerator;
        private readonly object _lock;

        public SafeDictionaryEnumerator(IEnumerator<TKey> keyEnumeratorParam, IEnumerator<TValue> valueEnumeratorParam,
            object @lock)
        {
            _keyEnumerator = keyEnumeratorParam;
            _valueEnumerator = valueEnumeratorParam;
            _lock = @lock;
            Monitor.Enter(_lock);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Monitor.Exit(_lock);
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            return _keyEnumerator.MoveNext() && _valueEnumerator.MoveNext();
        }

        public void Reset()
        {
            _keyEnumerator.Reset();
            _valueEnumerator.Reset();
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get { return new KeyValuePair<TKey, TValue>(_keyEnumerator.Current, _valueEnumerator.Current); }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}