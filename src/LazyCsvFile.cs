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
    using System.IO;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    ///     LazyCsvFile options.
    /// </summary>
    [Flags]
    public enum LazyCsvFileOptions
    {
        /// <summary>
        ///     None.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Decompress the input file regardless of whether the 'magic byte' is present.
        /// </summary>
        ForceGZip = 1,

        /// <summary>
        ///     Prevent reallocation of line strings.
        /// </summary>
        PreventLineReallocation = 2,
    }

    /// <summary>
    ///     Simplifies reading of CSV files via <see cref="LazyCsvLine"/>.
    /// </summary>
    public sealed class LazyCsvFile : IDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyCsvFile"/> class.
        /// </summary>
        /// <param name="file">The file containing the CSV to read.</param>
        /// <param name="lineSlack">The amount of slack space for each line.</param>
        public LazyCsvFile(string file, int lineSlack)
            : this(file, lineSlack, LazyCsvFileOptions.None)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyCsvFile"/> class.
        /// </summary>
        /// <param name="file">The file containing the CSV to read.</param>
        /// <param name="options">The reader options.</param>
        public LazyCsvFile(string file, LazyCsvFileOptions options)
            : this(file, 0, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyCsvFile"/> class.
        /// </summary>
        /// <param name="file">The file containing the CSV to read.</param>
        /// <param name="lineSlack">The amount of slack space for each line.</param>
        /// <param name="options">The reader options.</param>
        public LazyCsvFile(string file, int lineSlack, LazyCsvFileOptions options)
        {
            File = file;
            LineSlack = lineSlack;
            Options = options;

            using (var csv = new CsvStreamReader(file, options))
            {
                HeaderDictionary = csv.StreamReader.ReadLine().Split(',')
                    .Select((x, i) => new KeyValuePair<string, int>(x, i))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the end of the file has been reached.
        /// </summary>
        public bool EndOfFile => Reader?.StreamReader.EndOfStream ?? false;

        /// <summary>
        ///     Gets the file containing the CSV to read.
        /// </summary>
        public string File { get; }

        /// <summary>
        ///     Gets the CSV file headers.
        /// </summary>
        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(HeaderDictionary);

        /// <summary>
        ///     Gets the amount of slack space for each line.
        /// </summary>
        public int LineSlack { get; }

        /// <summary>
        ///     Gets the reader options.
        /// </summary>
        public LazyCsvFileOptions Options { get; }

        private Dictionary<string, int> HeaderDictionary { get; }
        private CsvStreamReader Reader { get; set; }

        /// <summary>
        ///     Disposes of this <see cref="LazyCsvFile"/>.
        /// </summary>
        public void Dispose()
        {
            Reader?.Dispose();
        }

        /// <summary>
        ///     Reads and returns all lines from the file into memory.
        /// </summary>
        /// <returns>The read lines.</returns>
        public IEnumerable<LazyCsvLine> ReadAllLines()
        {
            using (var csv = new CsvStreamReader(File, Options))
            {
                // discard headers
                csv.StreamReader.ReadLine();

                List<LazyCsvLine> lines = new List<LazyCsvLine>();

                while (!csv.StreamReader.EndOfStream)
                {
                    lines.Add(new LazyCsvLine(csv.StreamReader.ReadLine(), HeaderDictionary, LineSlack, Options.HasFlag(LazyCsvFileOptions.PreventLineReallocation)));
                }

                return lines;
            }
        }

        /// <summary>
        ///     Reads and returns the next line from the file.
        /// </summary>
        /// <returns>The read line.</returns>
        public LazyCsvLine ReadLine()
        {
            if (Reader == null)
            {
                Reader = new CsvStreamReader(File, Options);
                Reader.StreamReader.ReadLine(); // discard headers
            }

            return new LazyCsvLine(Reader.StreamReader.ReadLine(), HeaderDictionary, LineSlack, Options.HasFlag(LazyCsvFileOptions.PreventLineReallocation));
        }

        /// <summary>
        ///     Resets the current position in the file.
        /// </summary>
        public void ResetPosition() => Reader = new CsvStreamReader(File, Options);

        private sealed class CsvStreamReader : IDisposable
        {
            private readonly byte[] gzipFlags = new byte[] { 0x1F, 0x8B };

            public CsvStreamReader(string file, LazyCsvFileOptions options)
            {
                FileStream = new FileStream(file, FileMode.Open);

                if (options.HasFlag(LazyCsvFileOptions.ForceGZip) || IsGZipped(FileStream))
                {
                    GZipStream = new GZipStream(FileStream, CompressionMode.Decompress);
                    StreamReader = new StreamReader(GZipStream);
                }
                else
                {
                    StreamReader = new StreamReader(FileStream);
                }
            }

            public StreamReader StreamReader { get; }
            private FileStream FileStream { get; }
            private GZipStream GZipStream { get; }

            public void Dispose()
            {
                StreamReader?.Dispose();
                GZipStream?.Dispose();
                FileStream?.Dispose();
            }

            private bool IsGZipped(FileStream fileStream)
            {
                var fileFlags = new byte[2];
                fileStream.Read(fileFlags, 0, 2);

                fileStream.Position = 0;

                return fileFlags.AsSpan().SequenceEqual(gzipFlags.AsSpan());
            }
        }
    }
}