using System;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Concurrency;
using Xunit;
using Xunit.Abstractions;

namespace TestProject
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            this._output = output;
        }
        

        [Fact]
        public async void Test1()
        {
            var actionActor = new ActionTaskBasedActor(TaskScheduler.FromCurrentSynchronizationContext());
            for (int i = 0; i < 10; i++)
            {
                await actionActor.PostAsync(() =>
                {
                    _output.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString());
                });
            }
        }

        [Fact]
        public void Test2()
        {
            var actionActor = new ActionTaskBasedActor(TaskScheduler.FromCurrentSynchronizationContext());
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() =>
                {
                    actionActor.Post(() =>
                    {
                        _output.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString());
                    });
                });
            }
        }


        [Fact]
        public async void Test3()
        {
            var testActor = new TestActor(new LimitedConcurrencyLevelTaskScheduler(1),_output);
            for (int i = 0; i < 10; i++)
            {
                testActor.Post(new PostObject());                
            }
            

        }

        

        public async Task TaskTest(ITestOutputHelper output)
        {
            output.WriteLine("sasdf");
        }
    }

    public class PostObject
    {
        
    }
    
    public class TestActor : TaskBasedActor<PostObject>
    {
        private ITestOutputHelper _outputHelper;
        public TestActor(TaskScheduler scheduler, ITestOutputHelper outputHelper) : base(scheduler)
        {
            _outputHelper = outputHelper;
        }
        
        protected override async Task ReceiveMessage(PostObject message, CancellationToken token)
        {
            var newGuid = Guid.NewGuid();
            _outputHelper.WriteLine($"come into current thread {newGuid}");
            await Task.Delay(1000);
            _outputHelper.WriteLine($"leave current thread {newGuid}");
            
        }
    }
}