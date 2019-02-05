﻿namespace LazyCsvFile
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

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

            double sum = 0;

            List<Line> usageLines = lines.Where(l =>
                l["lineItem/ProductCode"].ToString() == "AmazonEC2" &&
                l["lineItem/UsageType"].ToString() == "EU-BoxUsage:t2.medium").ToList();

            foreach (var line in usageLines)
            {
                sum += double.Parse(line["lineItem/UsageAmount"].ToString());
            }

            for (int i = 0; i < usageLines.Count(); i++)
            {
                var line = usageLines[i];
                line["lineItem/UsageAmount"] = "10.0";
            }

            sw.Stop();

            Console.WriteLine($"Iterated over {usageLines.Count()} lines of EC2 t2.medium usage with total usage {sum} in {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            sw.Start();

            sum = 0;

            usageLines = lines.Where(l =>
                l["lineItem/ProductCode"].ToString() == "AmazonEC2" &&
                l["lineItem/UsageType"].ToString() == "EU-BoxUsage:t2.medium").ToList();

            foreach (var line in usageLines)
            {
                var val = line["lineItem/UsageAmount"].ToString();
                sum += double.Parse(val);
            }

            sw.Stop();

            Console.WriteLine($"Iterated over {usageLines.Count()} lines of EC2 t2.medium usage with total usage {sum} in {sw.ElapsedMilliseconds}ms");

            //Span<char> line = new Memory<char>("1,\"2,3\",4,".ToCharArray()).Span;

            //int start = 0;
            //bool quoted = false;

            //var offsets = new List<(int start, int length)>();

            //for (int i = 0; i < line.Length; i++)
            //{
            //    char c = line[i];

            //    Console.WriteLine($"{i}/{line.Length}: {c}");

            //    if (c == '"' || c == '\'')
            //    {
            //        quoted = !quoted;
            //    }

            //    if (i == line.Length - 1)
            //    {
            //        if (c == ',')
            //        {
            //            offsets.Add((start, i - (start)));
            //            offsets.Add((start, 0));
            //        }
            //        else
            //        {
            //            offsets.Add((start, i - (start - 1)));
            //        }
            //    }
            //    else if (!quoted && c == ',')
            //    {
            //        offsets.Add((start, i - (start)));
            //        start = i + 1;
            //    }
            //}

            //Console.WriteLine($"{line.ToString()}");

            //foreach(var offset in offsets)
            //{
            //    Console.WriteLine($"[{offset.start}, {offset.length}]: {line.Slice(offset.start, offset.length).ToString()}");
            //}

            Console.ReadKey();
        }
    }
}