/*
    MIT License

    Copyright (c) 2019 JP Dillingham

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace LazyCsv
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     A lazily parsed line of CSV text.
    /// </summary>
    public sealed class LazyCsvLine
    {
        private readonly Dictionary<string, int> _headers;
        private Memory<Offset> _offsets;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyCsvLine"/> class.
        /// </summary>
        /// <param name="text">The line text.</param>
        /// <param name="headers">The headers from the file containing the line.</param>
        /// <param name="slack">The amount of slack space to pre-allocate for line growth (default: 10%)</param>
        /// <param name="preventReallocation">A value indicating whether the line should be allowed to grow beyond the allocated slack space, requiring a re-allocation.</param>
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

        /// <summary>
        ///     Gets the headers from the file containing the line.
        /// </summary>
        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(_headers);

        /// <summary>
        ///     Gets the initial amount of allocated slack space.
        /// </summary>
        public int InitialSlack { get; }

        /// <summary>
        ///     Gets the positional offsets for each CSV column within the line.
        /// </summary>
        public IEnumerable<Offset> Offsets => _offsets.ToArray();

        /// <summary>
        ///     Gets a value indicating whether the line should be allowed to grow beyond the allocated spack space, requiring a re-allocation.
        /// </summary>
        public bool PreventReallocation { get; }

        /// <summary>
        ///     Gets the current amount of allocated slack space.
        /// </summary>
        public int Slack { get; private set; }

        /// <summary>
        ///     Gets the line text.
        /// </summary>
        public Memory<char> Text { get; private set; }

        /// <summary>
        ///     Returns the text contained within the offset corresponding to the specified <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The name of the column for which the text is to be returned.</param>
        /// <returns>The text contained within the offset corresponding to the specified <paramref name="column"/></returns>
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

        /// <summary>
        ///     Returns the text contained within the specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The offset of the column for which the text is to be returned.</param>
        /// <returns>The text contained within the specified <paramref name="offset"/></returns>
        public string this[int offset]
        {
            get
            {
                return Text.Span.Slice(_offsets.Span[offset].Start, _offsets.Span[offset].Length).ToString();
            }

            set
            {
                var valueOffset = _offsets.Span[offset];
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

                off[offset].Length = valueOffset.Length += valueLengthDifference;

                var start = valueOffset.Start + valueOffset.Length + 1;

                for (int i = offset + 1; i < off.Length; i++)
                {
                    off[i].Start = start;
                    start += off[i].Length + 1;
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