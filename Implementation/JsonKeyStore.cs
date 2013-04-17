using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SierraLib.Analytics.Implementation
{
    public sealed class JsonKeyStore : IDictionary<string,object>, IDisposable
    {
        static readonly JsonSerializer Serializer = new JsonSerializer();
        static readonly JsonDeserializer Deserializer = new JsonDeserializer();


        public JsonKeyStore(string filename)
            : this(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            
        }

        public JsonKeyStore(Stream file)
        {
            File = file;
            FileReader = new StreamReader(File);
            FileWriter = new StreamWriter(File);
            
            LoadFile();
        }


        public Stream File
        { get; private set; }

        private Dictionary<string, object> Store = new Dictionary<string, object>();
        private StreamReader FileReader;
        private StreamWriter FileWriter;

        private void LoadFile()
        {
            var fileContents = FileReader.ReadToEnd();

            if(!string.IsNullOrWhiteSpace(fileContents))
                Store = Deserializer.Deserialize<Dictionary<string, object>>(new RestResponse() { Content = fileContents });
            else Store = new Dictionary<string,object>();
        }

        private void UpdateFile()
        {
            File.Seek(0, SeekOrigin.Begin);
            File.SetLength(0);
            File.Flush();

            var serialized = Serializer.Serialize(Store);

            FileWriter.Write(serialized);
            FileWriter.Flush();
        }

        public string GetSerializedValue()
        {
            File.Seek(0, SeekOrigin.Begin);
            return FileReader.ReadToEnd();
        }



        public void Add(string key, object value)
        {
            Store.Add(key, value);
            UpdateFile();
        }

        public bool ContainsKey(string key)
        {
            return Store.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return Store.Keys; }
        }

        public bool Remove(string key)
        {
            bool result = Store.Remove(key);
            if (result) UpdateFile();
            return result;
        }

        public bool TryGetValue(string key, out object value)
        {
            return Store.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return Store.Values; }
        }

        public object this[string key]
        {
            get
            {
                return Store[key];
            }
            set
            {
                Store[key] = value;
                UpdateFile();
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Store.Add(item.Key, item.Value);
            UpdateFile();
        }

        public void Clear()
        {
            Store.Clear();
            UpdateFile();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Store.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Store.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            bool result = Store.Remove(item.Key);
            if (result) UpdateFile();
            return result;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        public void Dispose()
        {
            FileReader.Close();
            FileWriter.Close();
            File.Close();
        }
    }
}
