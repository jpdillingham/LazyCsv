namespace LazyCsvFile
{
    using System;
    using System.Diagnostics;

    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Reading file...");
            var lines = new LineCollection(@"c:\file.csv");
            Console.WriteLine($"Done.");

            Console.WriteLine($"Headers: {string.Join(", ", lines.Headers)}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var len = 0;

            foreach (var line in lines)
            {
                len += line["identity/LineItemId"].ToString().Length;
            }

            sw.Stop();

            Console.WriteLine($"Iterated over {lines.Lines.Count} lines with total length {len} in {sw.ElapsedMilliseconds}ms");

            Console.ReadKey();
        }
    }
}
