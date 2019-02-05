namespace LazyCsvFile
{
    using System;
    using System.Diagnostics;

    public class Program
    {
        static LineCollection Lines { get; } = new LineCollection();

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading file...");
            Lines.LoadFrom(@"c:\file.csv");
            Console.WriteLine($"Done.");

            Console.WriteLine($"Headers: {string.Join(", ", Lines.Headers)}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var line in Lines)
            {
                Console.WriteLine(line["identity/LineItemId"]);
            }

            sw.Stop();

            Console.WriteLine($"Iterated over {Lines.Lines.Count} lines in {sw.ElapsedMilliseconds}ms");

            Console.ReadKey();
        }
    }
}
