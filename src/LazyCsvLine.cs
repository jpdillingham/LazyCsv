namespace LazyCsv
{
    using System;
    using System.Collections.Generic;

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

    public sealed class LazyCsvLine
    {
        private Memory<Offset> Offsets;

        private Memory<char> Text;

        private readonly Dictionary<string, int> Headers;
        private int Slack;

        public LazyCsvLine(string text, Dictionary<string, int> headers, int slack)
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
}
