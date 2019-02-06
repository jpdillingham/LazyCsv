namespace LazyCsvFile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    //public class Line
    //{
    //    public string[] Text;

    //    private readonly Dictionary<string, int> Headers;

    //    public Line(string text, Dictionary<string, int> headers)
    //    {
    //        Text = text.Split(',');
    //        Headers = headers;
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
    //            return Text[i];
    //        }
    //        set
    //        {
    //            Text[i] = value;
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return string.Join(',', Text);
    //    }
    //}

    public class Line
    {
        public (int start, int length)[] Offsets;

        public Memory<char> Text;

        private readonly Dictionary<string, int> Headers;
        private readonly int Slack;

        public Line(string text, Dictionary<string, int> headers, int slack)
        {
            Slack = slack;

            Text = new char[text.Length + slack];
            text.AsSpan().CopyTo(Text.Span);

            Span<char> span = stackalloc char[text.Length + slack];
            Text.Span.CopyTo(span);

            Headers = headers;
            Slack = slack;

            Offsets = new(int start, int length)[Headers.Count];

            bool quoted = false;
            int start = 0;
            int offsetNum = 0;

            for (int i = 0; i < Text.Length; i++)
            {
                char c = span[i];

                if (c == '"' || c == '\'')
                {
                    quoted = !quoted;
                }

                if (i == Text.Length - 1)
                {
                    if (c == ',')
                    {
                        Offsets[offsetNum] = ((start + 1, i - (start + 1)));
                        Offsets[offsetNum + 1] = ((start + 1, 0));

                        offsetNum += 2;
                    }
                    else
                    {
                        Offsets[offsetNum] = ((start + 1, i - (start + 0)));
                        offsetNum++;
                    }
                }
                else if (!quoted && c == ',')
                {
                    Offsets[offsetNum] = ((start, i - (start)));
                    offsetNum++;
                    start = i + 1;
                }
            }

            //// todo: populate offsets
            //Offsets.Add((start: 0, length: 52));
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
                return Text.Span.Slice(Offsets[i].start, Offsets[i].length).ToString();
            }
            set
            {
                Console.WriteLine($"-----------------");
                Console.WriteLine($"Current len: {Text.Length - 10}, {Text.Span.Slice(0, Text.Length - 10).ToString()}");

                // todo: figure out how to update Memory<T>
                var offset = Offsets[i];
                Console.WriteLine($"updating offset {i}: [{offset.start}, {offset.length}]");

                var leftChunk = (start: 0, length: offset.start);
                Console.WriteLine($"left chunk: [{leftChunk.start}, {leftChunk.length}]");

                var rightChunk = (start: offset.start + offset.length, length: Text.Length - Slack - (offset.start + offset.length));
                Console.WriteLine($"right chunk: [{rightChunk.start}, {rightChunk.length}]");

                Span<char> oldText = stackalloc char[Text.Length];
                Text.Span.CopyTo(oldText);

                var len = leftChunk.length + value.Length + rightChunk.length;
                Console.WriteLine($"new: {len}");
                Span<char> newText = stackalloc char[len];              

                oldText.Slice(leftChunk.start, leftChunk.length).CopyTo(newText.Slice(leftChunk.start, leftChunk.length));
                Console.WriteLine($"copied left chunk: {newText.ToString()}");

                //oldText.CopyTo(newText.Slice(offset.start, value.Length));
                value.AsSpan().CopyTo(newText.Slice(offset.start, value.Length));
                Console.WriteLine($"copied new value: {newText.ToString()}");

                oldText.Slice(rightChunk.start, rightChunk.length).CopyTo(newText.Slice(offset.start + value.Length, rightChunk.length));
                Console.WriteLine($"copied right chunk: {newText.ToString()}");

                Offsets[i] = (offset.start, value.Length);

                Text.Span.Clear();
                newText.CopyTo(Text.Span);

                Console.WriteLine($"Updated len: {Text.Length}, {Text}");
            }
        }

        public override string ToString() => Text.ToString();
    }

    public class LineCollection : IList<Line>
    {
        public int Count => ((IList<Line>)Lines).Count;
        public Dictionary<string, int> Headers { get; } = new Dictionary<string, int>();
        public bool IsReadOnly => ((IList<Line>)Lines).IsReadOnly;
        public List<Line> Lines { get; } = new List<Line>();
        Line IList<Line>.this[int index] { get => ((IList<Line>)Lines)[index]; set => ((IList<Line>)Lines)[index] = value; }

        public Line this[int i] => Lines[i];
        public string this[int i, string column] => Lines[i][Headers[column]].ToString();

        public LineCollection(string file, int slack)
        {
            using (var reader = new StreamReader(file))
            {
                Headers = reader.ReadLine().Split(',')
                    .Select((x, i) => new KeyValuePair<string, int>(x, i))
                    .ToDictionary(x => x.Key, x => x.Value);

                while (!reader.EndOfStream)
                {
                    Lines.Add(new Line(reader.ReadLine(), Headers, slack));
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