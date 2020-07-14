using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.CloudEvents
{
    public class TraceState : IDictionary<string, string>
    {
        private static readonly char[] _commaSeparator = new[] { ',' };
        private static readonly char[] _equalsSeparator = new[] { '=' };
        private readonly IList<KeyValuePair<string, string>> _list = new List<KeyValuePair<string, string>>();

        internal TraceState()
        {
        }

        internal void Parse(string traceState)
        {
            _list.Clear();

            var items = traceState.Split(_commaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(unparsedItem => unparsedItem.Split(_equalsSeparator, StringSplitOptions.RemoveEmptyEntries))
                .Where(item => item.Length == 2);

            foreach (var item in items)
                this[item[0]] = item[1];

            SetValue();
        }

        public string Value { get; private set; }

        public int Count => _list.Count;

        public string this[string key]
        {
            get
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                foreach (var item in _list)
                    if (item.Key == key)
                        return item.Value;

                throw new KeyNotFoundException("The given key was not present in the dictionary.");
            }
            set
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                foreach (var item in _list)
                {
                    if (item.Key == key)
                    {
                        _list.Remove(item);
                        break;
                    }
                }
                _list.Insert(0, new KeyValuePair<string, string>(key, value));
                SetValue();
            }
        }

        public void Add(string key, string value) =>
            ((ICollection<KeyValuePair<string, string>>)this).Add(new KeyValuePair<string, string>(key, value));

        public bool Remove(string key)
        {
            foreach (var item in _list)
            {
                if (item.Key == key && _list.Remove(item))
                {
                    SetValue();
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _list.Clear();
            SetValue();
        }

        public bool ContainsKey(string key) =>
            _list.Any(item => item.Key == key);

        public bool TryGetValue(string key, out string value)
        {
            foreach (var item in _list)
            {
                if (item.Key == key)
                {
                    value = item.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            foreach (var listItem in _list)
                if (listItem.Key == item.Key)
                    throw new ArgumentException("An item with the same key has already been added.");
            _list.Insert(0, item);
            SetValue();
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            if (_list.Remove(item))
            {
                SetValue();
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

        ICollection<string> IDictionary<string, string>.Keys => _list.Select(x => x.Key).ToArray();

        ICollection<string> IDictionary<string, string>.Values => _list.Select(x => x.Value).ToArray();

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) =>
            _list.Contains(item);

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) =>
            _list.CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() =>
            _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _list.GetEnumerator();

        private void SetValue() =>
            Value = string.Join(",", _list.Select(x => $"{x.Key}={x.Value}"));


        //private string _value;

        //public string Value => _value;

        //public IEnumerable<KeyValuePair<string, string>> List => null;

        //public void Update(string key, string value)
        //{

        //}

        //public void Add(string key, string value)
        //{

        //}

        //public void Delete(string key)
        //{

        //}
    }
}
