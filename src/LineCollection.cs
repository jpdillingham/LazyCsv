﻿namespace LazyCsvFile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    //public class Line
    //{
    //    public Memory<string> Text;

    //    private readonly Dictionary<string, int> Headers;

    //    public Line(string text, Dictionary<string, int> headers, int slack)
    //    {
    //        //Text = text.Split(',');
    //        Text = SplitCsvLine(text).ToArray();
    //        Headers = headers;
    //    }

    //    public List<string> SplitCsvLine(string s)
    //    {
    //        int i;
    //        int a = 0;
    //        int count = 0;
    //        List<string> str = new List<string>();
    //        for (i = 0; i < s.Length; i++)
    //        {
    //            switch (s[i])
    //            {
    //                case ',':
    //                    if ((count & 1) == 0)
    //                    {
    //                        str.Add(s.Substring(a, i - a));
    //                        a = i + 1;
    //                    }
    //                    break;
    //                case '"':
    //                case '\'': count++; break;
    //            }
    //        }
    //        str.Add(s.Substring(a));
    //        return str;
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
    //            return Text.Span[i];
    //        }
    //        set
    //        {
    //            Text.Span[i] = value;
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return string.Join(',', Text.Span.ToArray());
    //    }
    //}

    public struct Offset
    {
        public Offset(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start;
        public int Length;
    }

    public sealed class Line
    {
        private Memory<Offset> Offsets;

        private Memory<char> Text;

        private readonly Dictionary<string, int> Headers;
        private int Slack;

        public Line(string text, Dictionary<string, int> headers, int slack)
        {
            Slack = slack;
            Offsets = new Offset[headers.Count];
            Text = new char[text.Length + slack];
            text.AsSpan().CopyTo(Text.Span);

            Headers = headers;
            Slack = slack;

            ComputeOffsets();
        }

        private void ComputeOffsets()
        {
            Span<char> span = stackalloc char[Text.Length];
            Text.Span.CopyTo(span);

            Span<Offset> offsets = stackalloc Offset[Headers.Count];

            bool quoted = false;
            int start = 0;
            int offsetNum = 0;
            int len = Text.Length;

            for (int i = 0; i < len; i++)
            {
                char c = span[i];

                if (c == '"' || c == '\'')
                {
                    quoted = !quoted;
                }

                if (i == len - 1)
                {
                    if (c == ',')
                    {
                        offsets[offsetNum] = new Offset(start + 1, i - (start + 1));
                        offsets[offsetNum + 1] = new Offset(start + 1, 0);

                        offsetNum += 2;
                    }
                    else
                    {
                        offsets[offsetNum] = new Offset(start + 1, i - (start + 0));
                        offsetNum++;
                    }
                }
                else if (!quoted && c == ',')
                {
                    offsets[offsetNum] = new Offset(start, i - (start));
                    offsetNum++;
                    start = i + 1;
                }
            }

            offsets.CopyTo(Offsets.Span);
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
                return Text.Span.Slice(Offsets.Span[i].Start, Offsets.Span[i].Length).ToString();
            }
            set
            {
                var valueOffset = Offsets.Span[i];
                var valueLengthDifference = value.Length - valueOffset.Length;

                if (valueLengthDifference == 0)
                {
                    // if the length didn't change, just overwrite the data in place.
                    value.AsSpan().CopyTo(Text.Span.Slice(valueOffset.Start));
                    return;
                }

                var shiftChunkStart = valueOffset.Start + valueOffset.Length;
                var shiftChunkLength = Text.Span.Length - shiftChunkStart - Slack - valueLengthDifference;
                var shiftChunkDestination = shiftChunkStart + valueLengthDifference;

                Text.Span
                    .Slice(shiftChunkStart, shiftChunkLength)
                    .CopyTo(Text.Span.Slice(shiftChunkDestination));

                value.AsSpan().CopyTo(Text.Span.Slice(valueOffset.Start));

                Slack -= valueLengthDifference;

                Span<Offset> off = stackalloc Offset[Headers.Count];
                Offsets.Span.CopyTo(off);

                off[i].Length = valueOffset.Length += valueLengthDifference;

                var start = valueOffset.Start + valueOffset.Length + 1;

                for (int j = i + 1; j < off.Length; j++)
                {
                    off[j].Start = start;
                    start += off[j].Length + 1;
                }

                off.CopyTo(Offsets.Span);
            }
        }

        public override string ToString() => Text.Span.Slice(0, Text.Length - Slack).ToString();
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
            //var mem = File.ReadAllBytes(file);

            //using (var reader = new StreamReader(file))
            using (FileStream fileStream = File.Open(file, FileMode.Open))
            //using (MemoryStream memstr = new MemoryStream(mem))
            using (GZipStream inZip = new GZipStream(fileStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(inZip))
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