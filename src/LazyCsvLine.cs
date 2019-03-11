namespace LazyCsv
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class LazyCsvLine
    {
        private readonly Dictionary<string, int> _headers;
        private Memory<Offset> _offsets;

        public LazyCsvLine(string text, Dictionary<string, int> headers, int? slack = null, bool preventReallocation = false)
        {
            Slack = slack ?? (int)Math.Ceiling(text.Length * 0.1d); // default to 10% of string length
            InitialSlack = Slack;

            _headers = headers;
            _offsets = new Offset[headers.Count];

            Text = new char[text.Length + Slack];
            text.AsSpan().CopyTo(Text.Span);

            PreventReallocation = preventReallocation;

            ComputeOffsets();
        }

        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(_headers);
        public int InitialSlack { get; }
        public IEnumerable<Offset> Offsets => _offsets.ToArray();
        public bool PreventReallocation { get; }
        public int Slack { get; private set; }
        public Memory<char> Text { get; private set; }

        public string this[string column]
        {
            get
            {
                return this[_headers[column]];
            }
            set
            {
                this[_headers[column]] = value;
            }
        }

        public string this[int i]
        {
            get
            {
                return Text.Span.Slice(_offsets.Span[i].Start, _offsets.Span[i].Length).ToString();
            }
            set
            {
                var valueOffset = _offsets.Span[i];
                var valueLengthDifference = value.Length - valueOffset.Length;

                if (valueLengthDifference == 0)
                {
                    // if the length didn't change, just overwrite the data in place.
                    value.AsSpan().CopyTo(Text.Span.Slice(valueOffset.Start));
                    return;
                }
                else if (valueLengthDifference > Slack)
                {
                    // if the new data will exceed the amount of available slack, reallocate the backing string to accomodate the
                    // new value + slack
                    if (PreventReallocation)
                    {
                        throw new InvalidOperationException($"Number of characters to be added ({valueLengthDifference}) exceeds available slack ({Slack}) and PreventReallocation = true");
                    }

                    Memory<char> text = new char[Text.Length - Slack + valueLengthDifference + InitialSlack];
                    Text.Span.CopyTo(text.Span);
                    Text = text;
                    Slack = Slack + valueLengthDifference + InitialSlack;
                }

                // compute the start and length of the string chunk to be moved left or right to accomodate the new value
                var shiftChunkStart = valueOffset.Start + valueOffset.Length;
                var shiftChunkLength = Text.Span.Length - shiftChunkStart - Slack;

                // compute the position at which the moved chunk is to be inserted
                var shiftChunkDestination = shiftChunkStart + valueLengthDifference;

                Text.Span
                    .Slice(shiftChunkStart, shiftChunkLength)
                    .CopyTo(Text.Span.Slice(shiftChunkDestination));

                value.AsSpan().CopyTo(Text.Span.Slice(valueOffset.Start));

                Slack -= valueLengthDifference;

                Span<Offset> off = stackalloc Offset[_headers.Count];
                _offsets.Span.CopyTo(off);

                off[i].Length = valueOffset.Length += valueLengthDifference;

                var start = valueOffset.Start + valueOffset.Length + 1;

                for (int j = i + 1; j < off.Length; j++)
                {
                    off[j].Start = start;
                    start += off[j].Length + 1;
                }

                off.CopyTo(_offsets.Span);
            }
        }

        public override string ToString() => Text.Span.Slice(0, Text.Length - Slack).ToString();

        private void ComputeOffsets()
        {
            int len = Text.Length - Slack;

            Span<char> span = stackalloc char[len];
            Text.Span.Slice(0, len).CopyTo(span);

            Span<Offset> offsets = stackalloc Offset[_headers.Count];

            bool quoted = false;
            int start = 0;
            int offsetNum = 0;

            for (int i = 0; i < len; i++)
            {
                switch (span[i])
                {
                    case ',':
                        if (!quoted)
                        {
                            offsets[offsetNum] = new Offset(start, i - start);
                            offsetNum++;
                            start = i + 1;
                        }
                        break;

                    case '\'':
                    case '"':
                        quoted = !quoted;
                        break;
                }
            }

            offsets[offsetNum] = new Offset(start, len - start);
            offsets.CopyTo(_offsets.Span);
        }

        public struct Offset
        {
            public int Length;

            public int Start;

            public Offset(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }
    }
}