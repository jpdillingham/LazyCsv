namespace LazyCsvFile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Line
    {
        public string[] Text;

        private readonly Dictionary<string, int> Headers;

        public Line(string text, Dictionary<string, int> headers)
        {
            Text = text.Split(',');
            Headers = headers;
        }

        public string this[string column]
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

        public string this[int i]
        {
            get
            {
                return Text[i];
            }
            set
            {
                Text[i] = value;
            }
        }

        public override string ToString()
        {
            return string.Join(',', Text);
        }
    }

    //public struct Line
    //{
    //    public (int start, int length)[] Offsets;

    //    public Memory<char> Text;

    //    private readonly Dictionary<string, int> Headers;

    //    public Line(string text, Dictionary<string, int> headers)
    //    {
    //        Text = new Memory<char>(text.ToCharArray());
    //        Headers = headers;

    //        Offsets = new(int start, int length)[Headers.Count];

    //        var span = Text.Span;
    //        bool quoted = false;
    //        int start = 0;
    //        int offsetNum = 0;

    //        for (int i = 0; i < span.Length; i++)
    //        {
    //            char c = span[i];

    //            if (c == '"' || c == '\'')
    //            {
    //                quoted = !quoted;
    //            }

    //            if (i == span.Length - 1)
    //            {
    //                if (c == ',')
    //                {
    //                    Offsets[offsetNum] = ((start + 1, i - (start + 1)));
    //                    Offsets[offsetNum + 1] = ((start + 1, 0));

    //                    offsetNum += 2;
    //                }
    //                else
    //                {
    //                    Offsets[offsetNum] = ((start + 1, i - (start + 0)));
    //                    offsetNum++;
    //                }
    //            }
    //            else if (!quoted && c == ',')
    //            {
    //                Offsets[offsetNum] = ((start + 1, i - (start + 1)));
    //                offsetNum++;
    //                start = i;
    //            }
    //        }

    //        //// todo: populate offsets
    //        //Offsets.Add((start: 0, length: 52));
    //    }

    //    public string this[string column]
    //    {
    //        get
    //        {
    //            return this[Headers[column]];
    //        }
    //        set
    //        {
    //            this[Headers[column]] = value;
    //        }
    //    }

    //    public string this[int i]
    //    {
    //        get
    //        {
    //            return Text.Slice(Offsets[i].start, Offsets[i].length).ToString();
    //        }
    //        set
    //        {
    //            // todo: figure out how to update Memory<T>
    //            var offset = Offsets[i];

    //            var currentvalue = Text.Slice(offset.start, offset.length);

    //            var leftChunk = (start: 0, length: offset.start);
    //            var rightChunk = (start: offset.start + offset.length, length: Text.Length - (offset.start + offset.length));

    //            Span<char> newText = stackalloc char[leftChunk.length + value.Length + rightChunk.length];

    //            Text.Span.Slice(leftChunk.start, leftChunk.length).CopyTo(newText.Slice(leftChunk.start, leftChunk.length));
    //            value.AsSpan().CopyTo(newText.Slice(offset.start, value.Length));
    //            Text.Span.Slice(rightChunk.start, rightChunk.length).CopyTo(newText.Slice(offset.start + value.Length, rightChunk.length));

    //            Offsets[i] = (offset.start, value.Length);
    //            newText.CopyTo(Text.Span);
    //        }
    //    }

    //    public override string ToString() => Text.ToString();
    //}

    public class LineCollection : IList<Line>
    {
        public int Count => ((IList<Line>)Lines).Count;
        public Dictionary<string, int> Headers { get; } = new Dictionary<string, int>();
        public bool IsReadOnly => ((IList<Line>)Lines).IsReadOnly;
        public List<Line> Lines { get; } = new List<Line>();
        Line IList<Line>.this[int index] { get => ((IList<Line>)Lines)[index]; set => ((IList<Line>)Lines)[index] = value; }

        public Line this[int i] => Lines[i];
        public string this[int i, string column] => Lines[i][Headers[column]].ToString();

        public LineCollection(string file)
        {
            using (var reader = new StreamReader(file))
            {
                Headers = reader.ReadLine().Split(',')
                    .Select((x, i) => new KeyValuePair<string, int>(x, i))
                    .ToDictionary(x => x.Key, x => x.Value);

                while (!reader.EndOfStream)
                {
                    Lines.Add(new Line(reader.ReadLine(), Headers));
                }
            }
        }

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