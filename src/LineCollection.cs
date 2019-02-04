namespace LazyCsvFile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class LineCollection
    {
        public struct ColumnOffset
        {
            public int Start;
            public int Length;
        }

        public struct Line
        {
            public Line(string text, Dictionary<string, int> headers)
            {
                Text = new Memory<char>(text.ToCharArray());
                Offsets = new List<ColumnOffset>();
                Headers = headers;

                // todo: populate offsets
                Offsets.Add(new ColumnOffset() { Start = 0, Length = 52 });
            }

            private readonly Dictionary<string, int> Headers;
            public List<ColumnOffset> Offsets;
            public Memory<char> Text;

            public string this[string name]
            {
                get
                {
                    // todo: look up the name in headers, use that index to fetch from Offsets, use those offsets to pull the string from Text
                    var offset = Offsets[Headers[name]];
                    return Text.Slice(offset.Start, offset.Length).ToString();
                }
                set
                {
                    // todo: figure out how to update Memory<T>
                }
            }
        }

        public Dictionary<string, int> Headers { get; } = new Dictionary<string, int>();
        public List<Line> Lines { get; } = new List<Line>();

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
    }
}
