namespace LazyCsv.Benchmarks
{
    using System;
    using System.Collections.Generic;

    public class StringArrayLine
    {
        public Memory<string> Text;

        private readonly Dictionary<string, int> Headers;

        public StringArrayLine(string text, Dictionary<string, int> headers, int slack)
        {
            Text = SplitCsvLine(text).ToArray();
            Headers = headers;
        }

        public List<string> SplitCsvLine(string s)
        {
            int i;
            int a = 0;
            int count = 0;
            List<string> str = new List<string>();
            for (i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case ',':
                        if ((count & 1) == 0)
                        {
                            str.Add(s.Substring(a, i - a));
                            a = i + 1;
                        }
                        break;
                    case '"':
                    case '\'': count++; break;
                }
            }
            str.Add(s.Substring(a));
            return str;
        }

        public string this[string column]
        {
            get
            {
                return this[Headers[column]];
            }
            set
            {
                this[Headers[column]] = value;
            }
        }

        public string this[int i]
        {
            get
            {
                return Text.Span[i];
            }
            set
            {
                Text.Span[i] = value;
            }
        }

        public override string ToString()
        {
            return string.Join(',', Text.Span.ToArray());
        }
    }
}