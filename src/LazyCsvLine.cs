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
        private Memory<Offset> _offsets;
        private Memory<char> _text;
        private readonly Dictionary<string, int> _headers;
        private readonly int _initialSlack;
        private int _slack;
        private readonly bool _preventReallocation;

        public int Slack => _slack;
        public Offset[] Offsets => _offsets.ToArray();

        public LazyCsvLine(string text, Dictionary<string, int> headers, int slack, bool preventReallocation)
        {
            _slack = slack;
            _offsets = new Offset[headers.Count];
            _text = new char[text.Length + slack];
            text.AsSpan().CopyTo(_text.Span);

            _headers = headers;
            _preventReallocation = preventReallocation;

            _initialSlack = Slack;
            _slack = slack;

            ComputeOffsets();
        }

        private void ComputeOffsets()
        {
            Span<char> span = stackalloc char[_text.Length];
            _text.Span.CopyTo(span);

            Span<Offset> offsets = stackalloc Offset[_headers.Count];

            bool quoted = false;
            int start = 0;
            int offsetNum = 0;
            int len = _text.Length - _slack;

            for (int i = 0; i < len; i++)
            {
                char c = span[i];

                if (c == '"' || c == '\'')
                {
                    quoted = !quoted;
                }

                // last character
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
                        offsets[offsetNum] = new Offset(start, i - (start - 1));
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

            offsets.CopyTo(_offsets.Span);
        }

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
                return _text.Span.Slice(_offsets.Span[i].Start, _offsets.Span[i].Length).ToString();
            }
            set
            {
                var valueOffset = _offsets.Span[i];
                var valueLengthDifference = value.Length - valueOffset.Length;
                bool reallocated = false;

                if (valueLengthDifference == 0)
                {
                    // if the length didn't change, just overwrite the data in place.
                    value.AsSpan().CopyTo(_text.Span.Slice(valueOffset.Start));
                    return;
                }
                else if (valueLengthDifference > _slack)
                {
                    if (_preventReallocation)
                    {
                        throw new InvalidOperationException($"Number of characters to be added ({valueLengthDifference}) exceeds available slack ({Slack}) and PreventReallocation = true");
                    }

                    Memory<char> newText = new char[_text.Length + valueLengthDifference - _slack + _initialSlack];
                    _text.Span.CopyTo(newText.Span);
                    _text = newText;
                    reallocated = true;
                }

                var shiftChunkStart = valueOffset.Start + valueOffset.Length;
                var shiftChunkLength = _text.Span.Length - shiftChunkStart - _slack - valueLengthDifference;
                var shiftChunkDestination = shiftChunkStart + valueLengthDifference;

                _text.Span
                    .Slice(shiftChunkStart, shiftChunkLength)
                    .CopyTo(_text.Span.Slice(shiftChunkDestination));

                value.AsSpan().CopyTo(_text.Span.Slice(valueOffset.Start));

                if (reallocated)
                {
                    _slack = _initialSlack;
                }
                else
                {
                    _slack -= valueLengthDifference;
                }
                
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

        public override string ToString() => _text.Span.Slice(0, _text.Length - Slack).ToString();
    }
}
