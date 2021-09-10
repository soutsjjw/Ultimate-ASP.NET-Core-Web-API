using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Entities.Models
{
    public class Entity : DynamicObject, IXmlSerializable, IDictionary<string, object>
    {
        #region IDictionary<string, object>

        // private readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();
        Dictionary<string, object> dictionary;

        public Entity()
        {
            dictionary = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                object value;
                if (dictionary.TryGetValue(key, out value))
                    return value;
                value = dictionary[key] = null;
                return value;
            }
            set
            {
                ((IDictionary<string, object>)dictionary)[key] = value;
            }
        }

        public ICollection<string> Keys => ((IDictionary<string, object>)dictionary).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>)dictionary).Values;

        public int Count => dictionary.Count();

        public bool IsReadOnly => ((IDictionary<string, object>)dictionary).IsReadOnly;

        public void Add(string key, object value)
        {
            dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)dictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IDictionary<string, object>)dictionary).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)dictionary).Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)dictionary).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return ((IDictionary<string, object>)dictionary).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, object>)dictionary).GetEnumerator();
        }

        #endregion IDictionary<string, object>

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            lock (this)
            {
                foreach(var item in dictionary)
                {
                    writer.WriteElementString(item.Key, item.Value?.ToString());
                }
            }
        }

        #endregion IXmlSerializable
    }
}
