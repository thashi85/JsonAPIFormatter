using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPISerializer.Models
{
    public class Reference:IDictionary<string,Object>
    {
        private readonly IDictionary<string, object> _ref = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                return _ref[key];
            }

            set
            {
                _ref[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return _ref.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _ref.IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _ref.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return _ref.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _ref.Add(item);
        }

        public void Add(string key, object value)
        {
            _ref.Add(key, value);
        }

        public void Clear()
        {
            _ref.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _ref.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _ref.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _ref.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _ref.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _ref.Remove(item);
        }

        public bool Remove(string key)
        {
            return _ref.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _ref.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ref.GetEnumerator();
        }
    }
}
