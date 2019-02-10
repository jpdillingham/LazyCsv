﻿namespace LazyCsvFile
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class Program
    {
        static void Main(string[] args)
        {
            //using (var file = new StreamReader(@"C:\CUR\4005-.csv"))
            //using (var outf = new StreamWriter(@"C:\CUR\mediumfile.csv"))
            //{
            //    int count = 0;
            //    while (!file.EndOfStream && count < 1000)
            //    {
            //        count++;
            //        outf.WriteLine(file.ReadLine());
            //    }
            //}

            //return;

            Stopwatch s = new Stopwatch();
            s.Start();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine($"Reading file...");
            var lines = new LineCollection(@"c:\CUR\aws-cur-003.csv", 10);
            Console.WriteLine($"Done.");

            sw.Stop();
            Console.WriteLine($"Loaded file in {sw.ElapsedMilliseconds}ms");
            //Console.WriteLine($"Headers: {string.Join(", ", lines.Headers)}");

            sw.Reset();
            sw.Start();

            //double sum = 0;

            //var usageLines = lines.Where(l =>
            //    l["lineItem/ProductCode"].ToString() == "AmazonEC2" &&
            //    l["lineItem/UsageType"].ToString() == "EU-BoxUsage:t2.medium").ToList();

            //foreach (var line in usageLines)
            //{
            //    sum += double.Parse(line["lineItem/UsageAmount"].ToString());
            //}

            //for (int i = 0; i < lines.Count(); i++)
            //{
            //    var line = lines[i]; // inability to index this directly might be a bug
            //    line["lineItem/UsageAmount"] = "10.0";
            //    line["identity/LineItemId"] = "a";
            //}

            for (int i = 0; i < 1; i++)
            {
                foreach (var line in lines)
                {
                    //line["five"] = "!";
                    var ubr = line["lineItem/UnblendedRate"];
                    ubr = ubr == string.Empty ? "0" : ubr;

                    try
                    {
                        line["lineItem/UnblendedRate"] = (decimal.Parse(ubr) * 2).ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse value '{ubr}': {ex.Message}");
                    }
                    var rid = line["lineItem/ResourceId"];
                    line["lineItem/ResourceId"] = rid == string.Empty ? "EMPTY" : rid;
                }
            }


            sw.Stop();

            Console.WriteLine($"Iterated over {lines.Count()} lines in {sw.ElapsedMilliseconds}ms");

            //sw.Reset();
            //sw.Start();

            //sum = 0;

            //foreach (var line in usageLines)
            //{
            //    var val = line["lineItem/UsageAmount"].ToString();
            //    sum += double.Parse(val);
            //}

            //sw.Stop();

            //Console.WriteLine($"Iterated over {usageLines.Count()} lines of EC2 t2.medium usage with total usage {sum} in {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            sw.Start();

            using (var writer = new System.IO.StreamWriter(@"C:\CUR\file.out.csv"))
            {
                foreach (var line in lines)
                {
                    //Console.WriteLine(line);
                    writer.WriteLine(line);
                }
            }

            sw.Stop();
            Console.WriteLine($"Saved {lines.Count} lines to output file in {sw.ElapsedMilliseconds}ms");

            s.Stop();
            Console.WriteLine($"Total time: {s.ElapsedMilliseconds}ms");

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
