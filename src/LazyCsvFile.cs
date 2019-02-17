using System;
using System.Collections;
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
        private readonly byte[] gzipFlags = new byte[] { 0x1F, 0x8B };

        public string File { get; }
        public LazyCsvFileOptions Options { get; }
        public int LineSlack { get; }
        public IReadOnlyDictionary<string, int> Headers => new ReadOnlyDictionary<string, int>(HeaderDictionary);
        private Dictionary<string, int> HeaderDictionary { get; } = new Dictionary<string, int>();

        private FileStream FileStream { get; }
        private GZipStream GZipStream { get; }
        private StreamReader StreamReader { get; }

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

            HeaderDictionary = StreamReader.ReadLine().Split(',')
                .Select((x, i) => new KeyValuePair<string, int>(x, i))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private bool IsGZipped(FileStream fileStream)
        {
            var fileFlags = new byte[2];
            fileStream.Read(fileFlags, 0, 2);

            fileStream.Position = 0;

            return fileFlags.AsSpan().SequenceEqual(gzipFlags.AsSpan());
        }

        public IEnumerable<LazyCsvLine> ReadAllLines()
        {
            // rewind the reader to the beginning of the file
            FileStream.Position = 0;
            StreamReader.DiscardBufferedData();

            // discard headers
            StreamReader.ReadLine();

            List<LazyCsvLine> lines = new List<LazyCsvLine>();

            while (!StreamReader.EndOfStream)
            {
                lines.Add(new LazyCsvLine(StreamReader.ReadLine(), HeaderDictionary, LineSlack));
            }

            return lines;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    StreamReader?.Dispose();
                    GZipStream?.Dispose();
                    FileStream?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LazyCsvFile() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
