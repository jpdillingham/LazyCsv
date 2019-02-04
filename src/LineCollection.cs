namespace LazyCsvFile
{
    using System.Collections.Generic;
    using System.IO;

    public class LineCollection
    {
        public List<string> Headers { get; } = new List<string>();
        public List<string> Lines { get; } = new List<string>();

        public void LoadFrom(string file)
        {
            using (var reader = new StreamReader(file))
            {
                var headers = reader.ReadLine();
                Headers.AddRange(headers.Split(','));

                while (!reader.EndOfStream)
                {
                    Lines.Add(reader.ReadLine());
                }
            }
        }
    }
}
