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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LazyCsv
{
    [Flags]
    public enum LazyCsvFileOptions
    {
        None = 0,
        ForceGZip = 1,
        PreventLineReallocation = 2,
    }

    public class LazyCsvFile : IDisposable
    {
        private bool disposedValue = false;

        public LazyCsvFile(string file, int lineSlack)
            : this(file, lineSlack, LazyCsvFileOptions.None)
        {
        }

        public LazyCsvFile(string file, LazyCsvFileOptions options)
            : this(file, 0, options)
        {
        }

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

        public bool EndOfFile => Reader?.StreamReader.EndOfStream ?? false;
        public string File { get; }
        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(HeaderDictionary);
        public int LineSlack { get; }
        public LazyCsvFileOptions Options { get; }
        private Dictionary<string, int> HeaderDictionary { get; } = new Dictionary<string, int>();

        private CsvStreamReader Reader { get; set; }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

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

        public LazyCsvLine ReadLine()
        {
            if (Reader == null)
            {
                Reader = new CsvStreamReader(File, Options);
                Reader.StreamReader.ReadLine(); // discard headers
            }

            return new LazyCsvLine(Reader.StreamReader.ReadLine(), HeaderDictionary, LineSlack, Options.HasFlag(LazyCsvFileOptions.PreventLineReallocation));
        }

        public void ResetPosition() => Reader = new CsvStreamReader(File, Options);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Reader?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. ~LazyCsvFile() {
        // // Do not change this code. Put cleanup code in Dispose(bool disposing) above. Dispose(false); }
        private class CsvStreamReader : IDisposable
        {
            private readonly byte[] gzipFlags = new byte[] { 0x1F, 0x8B };

            private bool disposedValue = false;

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

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        StreamReader?.Dispose();
                        GZipStream?.Dispose();
                        FileStream?.Dispose();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            private bool IsGZipped(FileStream fileStream)
            {
                var fileFlags = new byte[2];
                fileStream.Read(fileFlags, 0, 2);

                fileStream.Position = 0;

                return fileFlags.AsSpan().SequenceEqual(gzipFlags.AsSpan());
            }

            // To detect redundant calls
            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. ~CsvStream()
            // { // Do not change this code. Put cleanup code in Dispose(bool disposing) above. Dispose(false); }
        }
    }
}