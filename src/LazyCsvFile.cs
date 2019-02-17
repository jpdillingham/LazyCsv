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

            using (var csv = new CsvStream(file, options))
            {
                HeaderDictionary = csv.StreamReader.ReadLine().Split(',')
                    .Select((x, i) => new KeyValuePair<string, int>(x, i))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public string File { get; }
        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(HeaderDictionary);
        public int LineSlack { get; }
        public LazyCsvFileOptions Options { get; }
        private Dictionary<string, int> HeaderDictionary { get; } = new Dictionary<string, int>();

        private CsvStream Stream { get; set; }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        public IEnumerable<LazyCsvLine> ReadAllLines()
        {
            using (var csv = new CsvStream(File, Options))
            {
                // discard headers
                csv.StreamReader.ReadLine();

                List<LazyCsvLine> lines = new List<LazyCsvLine>();

                while (!csv.StreamReader.EndOfStream)
                {
                    lines.Add(new LazyCsvLine(csv.StreamReader.ReadLine(), HeaderDictionary, LineSlack));
                }

                return lines;
            }
        }

        public void ResetPosition() => Stream = new CsvStream(File, Options);

        public bool EndOfFile => Stream?.StreamReader.EndOfStream ?? false;

        public LazyCsvLine ReadLine()
        {
            if (Stream == null)
            {
                Stream = new CsvStream(File, Options);
            }

            return new LazyCsvLine(Stream.StreamReader.ReadLine(), HeaderDictionary, LineSlack);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stream?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. ~LazyCsvFile() {
        // // Do not change this code. Put cleanup code in Dispose(bool disposing) above. Dispose(false); }
        private class CsvStream : IDisposable
        {
            private readonly byte[] gzipFlags = new byte[] { 0x1F, 0x8B };

            private bool disposedValue = false;

            public CsvStream(string file, LazyCsvFileOptions options)
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