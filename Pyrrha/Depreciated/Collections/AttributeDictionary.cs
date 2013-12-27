#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Collections.Enumerators;

#endregion

namespace Pyrrha.Collections
{
    public class AttributeDictionary : IDictionary<string , BlockAttribute>
    {
        private IList<string> _keys;
        private IList<BlockAttribute> _values;
        private readonly object _lock = new object();

        public int Count
        {
            get { return _keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return _keys.IsReadOnly || _values.IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return _keys ?? (_keys = new List<string>()); }
        }

        public ICollection<BlockAttribute> Values
        {
            get { return _values ?? (_values = new List<BlockAttribute>()); }
        }

        public BlockAttribute this[string key]
        {
            get
            {
                if (!_keys.Contains(key)) throw new KeyNotFoundException(key);
                return _values[_keys.IndexOf(key)];
            }
            set
            {
                if (!_keys.Contains(key)) throw new KeyNotFoundException(key);
                _values[_keys.IndexOf(key)] = value;
            }
        }

        public void Add(KeyValuePair<string , AttributeReference> item)
        {
            Add(item.Key , item.Value);
        }

        public void Add(KeyValuePair<string , BlockAttribute> item)
        {
            Add(item.Key , item.Value);
        }

        public void Add(string key , AttributeReference value)
        {
            if (Keys.Contains(key))
                throw new ArgumentException(string.Format("An element with the same key already exists\n Key:{0}" , key));
            Add(key , new BlockAttribute(value));
        }

        public void Add(string key , BlockAttribute value)
        {
            if (Keys.Contains(key))
                throw new ArgumentException(string.Format("An element with the same key already exists\n Key:{0}" , key));
            _keys.Add(key);
            _values.Add(value);
            _attributeCollection.AppendAttribute(value.OriginalAttributeReference);
        }

        public bool TryGetValue(string key , out BlockAttribute value)
        {
            if (!_keys.Contains(key))
            {
                value = null;
                return false;
            }
            value = _values[_keys.IndexOf(key)];
            return true;
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string , BlockAttribute> item)
        {
            return _keys.Contains(item.Key) && _values.Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<string , BlockAttribute>[] array , int arrayIndex)
        {
            for (int i = arrayIndex; i < _keys.Count - 1; i++)
                array[i] = new KeyValuePair<string , BlockAttribute>(_keys[i] , _values[i]);
        }

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        public IEnumerator<KeyValuePair<string , BlockAttribute>> GetEnumerator()
        {
            return new SafeDictionaryEnumerator<string , BlockAttribute>(_keys.GetEnumerator() , _values.GetEnumerator() , _lock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public AttributeDictionary(AttributeCollection attributeCollectionParameter)
        {
            _attributeCollection = attributeCollectionParameter;
            foreach (ObjectId objectId in attributeCollectionParameter)
            {
                using (
                    OpenCloseTransaction trans =
                        HostApplicationServices.WorkingDatabase.TransactionManager.StartOpenCloseTransaction())
                {
                    var attr = (AttributeReference)trans.GetObject(objectId , OpenMode.ForRead);
                    Keys.Add(attr.Tag);
                    Values.Add(new BlockAttribute(attr));
                }
            }
        }

        private readonly AttributeCollection _attributeCollection;

        [Obsolete("Does Not Work!!, Returns False")]
        public bool Remove(KeyValuePair<string , BlockAttribute> item)
        {
            return false;
        }

        [Obsolete("Does Not Work!!, Returns False")]
        public bool Remove(string key)
        {
            return false;
        }
    }
}