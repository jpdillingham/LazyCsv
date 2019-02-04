namespace LazyCsvFile
{
    using System;

    public class Program
    {
        static LineCollection Lines { get; } = new LineCollection();

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading file...");
            Lines.LoadFrom(@"c:\file.csv");
            Console.WriteLine($"Done.");

            Console.WriteLine($"Headers: {string.Join(", ", Lines.Headers)}");

            Console.ReadKey();
        }
    }
}
