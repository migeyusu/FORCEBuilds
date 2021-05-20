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
            
            var actionActor = new ActionActor(TaskScheduler.FromCurrentSynchronizationContext());
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
            var actionActor = new ActionActor(TaskScheduler.FromCurrentSynchronizationContext());
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


        public async Task TaskTest(ITestOutputHelper output)
        {
            output.WriteLine("sasdf");
        }
    }

    public class ActionActor : TaskBasedActor<Action>
    {
        public ActionActor(TaskScheduler scheduler) : base(scheduler)
        {
        }

        protected override void ReceiveMessage(Action message)
        {
            message.Invoke();
        }
    }
}