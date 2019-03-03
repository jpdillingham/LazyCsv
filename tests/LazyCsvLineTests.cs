namespace LazyCsv.Tests
{
    using AutoFixture.Xunit2;
    using System;
    using System.Collections.Generic;
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
        [Fact(DisplayName = "Write reallocates when length exceeds slack and PreventReallocation is false")]
        public void Write_Reallocates_When_Length_Exceeds_Slack_And_PreventReallocation_Is_False()
        {
            var headers = new Dictionary<string, int>()
            {
                { "one", 0 },
                { "two", 1 },
                { "three", 2 }
            };

            var line = new LazyCsvLine("one, two, three", headers, 5, false);

            var ex1 = Record.Exception(() => line[0] = "the quick brown fox jumped over the lazy dog");
            var ex2 = Record.Exception(() => line[1] = "this is another long string");
            var ex3 = Record.Exception(() => line[2] = "aaaaaaaaaaaaaaa");

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Equal("the quick brown fox jumped over the lazy dog,this is another long string,aaaaaaaaaaaaaaa", line.ToString());
            Assert.Equal(5, line.Slack);
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
                Assert.Equal(offsets[i].Start, line.Offsets[i].Start);
                Assert.Equal(offsets[i].Length, line.Offsets[i].Length);
            }
        }
    }
}
