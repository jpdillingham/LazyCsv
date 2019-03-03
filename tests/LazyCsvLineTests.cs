namespace LazyCsv.Tests
{
    using AutoFixture.Xunit2;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class LazyCsvLineTests
    {
        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write throws on reallocation when PreventReallocation is true")]
        public void Write_Throws_On_Rellocation_When_PreventReallocation_True()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one, two, three", headers, 0, true);

            var ex = Record.Exception(() => line[0] = "four");

            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write takes up slack when length grows")]
        public void Write_Takes_Up_Slack_When_Length_Grows()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("1,2,3", headers, 3, false);

            var ex1 = Record.Exception(() => line[0] = "11");
            var ex2 = Record.Exception(() => line[1] = "22");
            var ex3 = Record.Exception(() => line[2] = "33");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("11,22,33", line.ToString());
            Assert.Equal(0, line.Slack);
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write leaves slack when length shrinks")]
        public void Write_Leaves_Slack_When_Length_Shrinks()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("11,22,33", headers, 0, false);

            var ex1 = Record.Exception(() => line[0] = "1");
            var ex2 = Record.Exception(() => line[1] = "2");
            var ex3 = Record.Exception(() => line[2] = "3");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("1,2,3", line.ToString());
            Assert.Equal(3, line.Slack);
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write reallocates when length exceeds slack and PreventReallocation is false")]
        public void Write_Reallocates_When_Length_Exceeds_Slack_And_PreventReallocation_Is_False()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("1,2,3", headers, 0, false);

            var ex1 = Record.Exception(() => line[0] = "11");
            var ex2 = Record.Exception(() => line[1] = "22");
            var ex3 = Record.Exception(() => line[2] = "33");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("11,22,33", line.ToString());
            Assert.Equal(0, line.Slack);
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write leaves slack 2")]
        public void Write_Leaves_Slack_2()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            // length = 147
            var line = new LazyCsvLine("Lorem ipsum dolor sit amet consectetur adipiscing elit.,Nunc a massa sit amet augue lacinia lacinia.,Aliquam scelerisque placerat turpis ut mollis.", headers, 0, false);

            var ex1 = Record.Exception(() => line[0] = "1");
            var ex2 = Record.Exception(() => line[1] = "2");
            var ex3 = Record.Exception(() => line[2] = "3");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("1,2,3", line.ToString()); // length = 5
            Assert.Equal(142, line.Slack); // 147 - 5
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write reallocates when length exceeds slack and PreventReallocation is false")]
        public void Write_Reallocates_When_Length_Exceeds_Slack_And_PreventReallocation_Is_False2()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("1,2,3", headers, 0, false);

            var ex1 = Record.Exception(() => line[0] = "Lorem ipsum dolor sit amet consectetur adipiscing elit.");
            var ex2 = Record.Exception(() => line[1] = "Nunc a massa sit amet augue lacinia lacinia.");
            var ex3 = Record.Exception(() => line[2] = "Aliquam scelerisque placerat turpis ut mollis.");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("Lorem ipsum dolor sit amet consectetur adipiscing elit.,Nunc a massa sit amet augue lacinia lacinia.,Aliquam scelerisque placerat turpis ut mollis.", line.ToString());
            Assert.Equal(0, line.Slack);
        }

        [Trait("Category", "Write")]
        [Fact(DisplayName = "Write reallocates when length exceeds slack and PreventReallocation is false")]
        public void Write_Shrinks()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one, two, three", headers, 5, false);

            var ex1 = Record.Exception(() => line[0] = "1");
            var ex2 = Record.Exception(() => line[1] = "2");
            var ex3 = Record.Exception(() => line[2] = "3");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("1,2,3", line.ToString());
            Assert.Equal(15, line.Slack);
        }

        [Trait("Category", "Instantiation")]
        [Fact(DisplayName = "Handles double quoted values properly")]
        public void Handles_Double_Quoted_Values_Properly()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one,\"t,\"w\",o\",three", headers, 5, false);

            Assert.Equal(3, line.Offsets.ToArray().Length);
            Assert.Equal("one", line[0]);
            Assert.Equal("\"t,\"w\",o\"", line[1]);
            Assert.Equal("three", line[2]);
        }

        [Trait("Category", "Instantiation")]
        [Fact(DisplayName = "Handles single quoted values properly")]
        public void Handles_Single_Quoted_Values_Properly()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one,'t,'w',o',three", headers, 5, false);

            Assert.Equal(3, line.Offsets.ToArray().Length);
            Assert.Equal("one", line[0]);
            Assert.Equal("'t,'w',o'", line[1]);
            Assert.Equal("three", line[2]);
        }

        [Trait("Category", "Instantiation")]
        [Fact(DisplayName = "Handles mixed quoted values properly")]
        public void Handles_Mixed_Quoted_Values_Properly()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one,'t,\"w\",o',\"th'r'ee\"", headers, 5, false);

            Assert.Equal(3, line.Offsets.ToArray().Length);
            Assert.Equal("one", line[0]);
            Assert.Equal("'t,\"w\",o'", line[1]);
            Assert.Equal("\"th'r'ee\"", line[2]);
        }

        [Trait("Category", "Instantiation")]
        [Theory(DisplayName = "Computes initial offsets properly"), AutoData]
        public void Computes_Initial_Offsets_Properly(string[] strings)
        {
            var headers = new Dictionary<string, int>();
            var offsets = new List<Offset>();

            for (int i = 0; i < strings.Length; i++)
            {
                headers.Add(strings[i], i);

                var prev = i == 0 ? new Offset(-1, 0) : offsets[i - 1];

                offsets.Add(new Offset(prev.Start + prev.Length + 1, strings[i].Length));
            }

            var line = new LazyCsvLine(string.Join(",", strings), headers, 5, false);

            for (int i = 0; i < offsets.Count; i++)
            {
                Assert.Equal(offsets[i].Start, line.Offsets.ToArray()[i].Start);
                Assert.Equal(offsets[i].Length, line.Offsets.ToArray()[i].Length);
            }
        }

        [Trait("Category", "Instantiation")]
        [Fact(DisplayName = "Computes simple initial offsets properly")]
        public void Computes_Simple_Initial_Offsets_Properly()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("1,2,", headers, 0, false);

            Assert.Equal(3, line.Offsets.ToArray().Length);
            Assert.Equal("1", line[0]);
            Assert.Equal("2", line[1]);
            Assert.Equal("", line[2]);
        }
    }
}
