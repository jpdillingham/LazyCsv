namespace LazyCsv.Benchmarks
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
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

            for (int i = 0; i < 10; i++)
            {
                Stopwatch s = new Stopwatch();
                s.Start();

                //for (int i = 0; i < 10000000; i++)
                //{
                //    var x = Guid.NewGuid().ToString();
                //}

                //s.Stop();
                //Console.WriteLine($"regular: {s.ElapsedMilliseconds}");

                //s.Reset();
                //s.Start();

                //for (int i = 0; i < 10000000; i++)
                //{
                //    var x = Guid.NewGuid().ToString().ToUpper();
                //}


                //s.Stop();
                //Console.WriteLine($"regular: {s.ElapsedMilliseconds}");

                //Console.ReadKey();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                //Console.WriteLine($"Reading file...");
                var file = new LazyCsvFile(@"c:\CUR\aws-cur-003.csv.gz", 10);
                //var lines = file.ReadAllLines();
                //Console.WriteLine($"Done.");

                sw.Stop();
                //Console.WriteLine($"Loaded file in {sw.ElapsedMilliseconds}ms");
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


                //for (int i = 0; i < 1; i++)
                //{
                //    foreach (var line in lines)
                //    {
                //        ////line["five"] = "!";
                //        var ubr = line["lineItem/UnblendedRate"];
                //        ubr = ubr == string.Empty ? "0" : ubr;

                //        try
                //        {
                //            //Console.WriteLine(line["identity/LineItemId"]);
                //            //Console.WriteLine(line["identity/LineItemId"]);
                //            line["lineItem/UnblendedRate"] = (decimal.Parse(ubr) * 2).ToString();
                //            var rid = line["lineItem/ResourceId"];
                //            line["lineItem/ResourceId"] = rid == string.Empty ? "EMPTY" : rid;
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Failed to parse value '{ubr}': {ex.Message}");
                //        }

                //    }
                //}

                int lines = 0;


                using (var fs = File.Open(@"C:\CUR\file.out.csv", FileMode.Create))
                //using (var gzip = new GZipStream(fs, CompressionLevel.Fastest))
                using (var writer = new StreamWriter(fs))
                {

                    while (!file.EndOfFile)
                    {
                        var line = file.ReadLine();

                        ////line["five"] = "!";
                        var ubr = line["lineItem/UnblendedRate"];
                        ubr = ubr == string.Empty ? "0" : ubr;

                        try
                        {
                            //Console.WriteLine(line["identity/LineItemId"]);
                            //Console.WriteLine(line["identity/LineItemId"]);
                            line["lineItem/UnblendedRate"] = (decimal.Parse(ubr) * 2).ToString();
                            var rid = line["lineItem/ResourceId"];
                            line["lineItem/ResourceId"] = rid == string.Empty ? "EMPTY" : rid;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse value '{ubr}': {ex.Message}");
                        }
                        lines++;

                        writer.WriteLine(line);
                    }
                }


                sw.Stop();

                //Console.WriteLine($"Iterated over {lines} lines in {sw.ElapsedMilliseconds}ms");

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

                //using (var fs = File.Open(@"C:\CUR\file.out.csv", FileMode.Create))
                ////using (var gzip = new GZipStream(fs, CompressionLevel.Fastest))
                //using (var writer = new StreamWriter(fs))
                //{
                //    foreach (var line in lines)
                //    {
                //        //Console.WriteLine(line);
                //        writer.WriteLine(line);
                //    }
                //}

                //sw.Stop();
                //Console.WriteLine($"Saved {lines.Count()} lines to output file in {sw.ElapsedMilliseconds}ms");

                s.Stop();
                Console.WriteLine($"Total time: {s.ElapsedMilliseconds}ms");

                file.Dispose();
            }

            Console.ReadKey();
        }
    }
}
