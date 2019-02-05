namespace LazyCsvFile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public struct Line
    {
        public List<(int start, int length)> Offsets;

        public Memory<char> Text;

        private readonly Dictionary<string, int> Headers;

        public Line(string text, Dictionary<string, int> headers)
        {
            Text = new Memory<char>(text.ToCharArray());

            Offsets = new List<(int start, int length)>();
            Headers = headers;

            // todo: populate offsets
            Offsets.Add((start: 0, length: 52));
        }

        public Memory<char> this[string column]
        {
            get
            {
                return this[Headers[column]];
            }
            set
            {
                this[Headers[column]] = value;
            }
        }

        public Memory<char> this[int i]
        {
            get
            {
                return Text.Slice(Offsets[i].start, Offsets[i].length);
            }
            set
            {
                // todo: figure out how to update Memory<T>
            }
        }
    }

    public class LineCollection : IList<Line>
    {
        public int Count => ((IList<Line>)Lines).Count;
        public Dictionary<string, int> Headers { get; } = new Dictionary<string, int>();
        public bool IsReadOnly => ((IList<Line>)Lines).IsReadOnly;
        public List<Line> Lines { get; } = new List<Line>();
        Line IList<Line>.this[int index] { get => ((IList<Line>)Lines)[index]; set => ((IList<Line>)Lines)[index] = value; }

        public string this[int i] => Lines[i].Text.ToString();
        public string this[int i, string column] => Lines[i][Headers[column]].ToString();

        public void Add(Line item)
        {
            ((IList<Line>)Lines).Add(item);
        }

        public void Clear()
        {
            ((IList<Line>)Lines).Clear();
        }

        public bool Contains(Line item)
        {
            return ((IList<Line>)Lines).Contains(item);
        }

        public void CopyTo(Line[] array, int arrayIndex)
        {
            ((IList<Line>)Lines).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Line> GetEnumerator()
        {
            return ((IList<Line>)Lines).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Line>)Lines).GetEnumerator();
        }

        public int IndexOf(Line item)
        {
            return ((IList<Line>)Lines).IndexOf(item);
        }

        public void Insert(int index, Line item)
        {
            ((IList<Line>)Lines).Insert(index, item);
        }

        public void LoadFrom(string file)
        {
            using (var reader = new StreamReader(file))
            {
                var headerLine = reader.ReadLine();
                var headers = headerLine.Split(',');

                for (int i = 0; i < headers.Length; i++)
                {
                    Headers.Add(headers[i], i);
                }

                while (!reader.EndOfStream)
                {
                    Lines.Add(new Line(reader.ReadLine(), Headers));
                }
            }
        }

        public bool Remove(Line item)
        {
            return ((IList<Line>)Lines).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Line>)Lines).RemoveAt(index);
        }
    }
}