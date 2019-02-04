namespace LazyCsvFile
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class LineCollection
    {
        public struct ColumnOffset
        {
            public int Start;
            public int Length;
        }

        public struct Line
        {
            public Line(string text)
            {
                Text = new Memory<char>(text.ToCharArray());
                Offsets = new List<ColumnOffset>();

                // todo: populate offsets
            }

            public List<ColumnOffset> Offsets;
            public Memory<char> Text;
        }

        public List<string> Headers { get; } = new List<string>();
        public List<Line> Lines { get; } = new List<Line>();

        public void LoadFrom(string file)
        {
            using (var reader = new StreamReader(file))
            {
                var headers = reader.ReadLine();
                Headers.AddRange(headers.Split(','));

                while (!reader.EndOfStream)
                {
                    Lines.Add(new Line(reader.ReadLine()));
                }
            }
        }
    }
}
