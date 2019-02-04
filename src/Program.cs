namespace LazyCsvFile
{
    using System;

    public class Program
    {
        static LineCollection lines { get; } = new LineCollection();

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading file...");
            lines.LoadFrom(@"c:\file.csv");
            Console.WriteLine($"Done.");

            Console.WriteLine($"Headers: {string.Join(", ", lines.Headers)}");

            Console.ReadKey();
        }
    }
}
