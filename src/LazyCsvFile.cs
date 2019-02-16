using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LazyCsv
{
    public class LazyCsvFile : IList<LazyCsvLine>
    {
        public int Count => ((IList<LazyCsvLine>)Lines).Count;
        public Dictionary<string, int> Headers { get; } = new Dictionary<string, int>();
        public bool IsReadOnly => ((IList<LazyCsvLine>)Lines).IsReadOnly;
        public List<LazyCsvLine> Lines { get; } = new List<LazyCsvLine>();
        LazyCsvLine IList<LazyCsvLine>.this[int index]
        {
            get => ((IList<LazyCsvLine>)Lines)[index];
            set => ((IList<LazyCsvLine>)Lines)[index] = value;
        }

        public LazyCsvLine this[int i]
        {
            get => Lines[i];
            set => Lines[i] = value;
        }

        public string this[int i, string column] => Lines[i][Headers[column]].ToString();

        public LazyCsvFile(string file, int slack)
        {
            //var mem = File.ReadAllBytes(file);

            //using (var sr = new StreamReader(file))
            //using (FileStream fileStream = File.Open(file, FileMode.Open))
            //using (MemoryStream memstr = new MemoryStream(mem))
            //using (GZipStream inZip = new GZipStream(fileStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(file))
            {
                Headers = reader.ReadLine().Split(',')
                    .Select((x, i) => new KeyValuePair<string, int>(x, i))
                    .ToDictionary(x => x.Key, x => x.Value);

                while (!reader.EndOfStream)
                {
                    Lines.Add(new LazyCsvLine(reader.ReadLine(), Headers, slack));
                }
            }
        }

        public void Add(LazyCsvLine item)
        {
            ((IList<LazyCsvLine>)Lines).Add(item);
        }

        public void Clear()
        {
            ((IList<LazyCsvLine>)Lines).Clear();
        }

        public bool Contains(LazyCsvLine item)
        {
            return ((IList<LazyCsvLine>)Lines).Contains(item);
        }

        public void CopyTo(LazyCsvLine[] array, int arrayIndex)
        {
            ((IList<LazyCsvLine>)Lines).CopyTo(array, arrayIndex);
        }

        public IEnumerator<LazyCsvLine> GetEnumerator()
        {
            return ((IList<LazyCsvLine>)Lines).GetEnumerator();
        }

        public int IndexOf(LazyCsvLine item)
        {
            return ((IList<LazyCsvLine>)Lines).IndexOf(item);
        }

        public void Insert(int index, LazyCsvLine item)
        {
            ((IList<LazyCsvLine>)Lines).Insert(index, item);
        }

        public bool Remove(LazyCsvLine item)
        {
            return ((IList<LazyCsvLine>)Lines).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<LazyCsvLine>)Lines).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<LazyCsvLine>)Lines).GetEnumerator();
        }
    }
}
