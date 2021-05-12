using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace TestProject
{
    public class UnitTest1
    {
        private ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test1()
        {
            var taskTest = TaskTest(output);
        }


        public async Task TaskTest(ITestOutputHelper output)
        { 
            output.WriteLine("sasdf");
        }
    }
}
