using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Concurrency;
using FORCEBuild.Helper;
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
                actionActor.PostAsync(() => { _output.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString()); });
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
                    actionActor.Post(() => { _output.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString()); });
                });
            }
        }


        [Fact]
        public async void ThrowException()
        {
            var testActor = new TestActor(new LimitedConcurrencyLevelTaskScheduler(1), _output);
            for (int i = 0; i < 10; i++)
            {
                await Task.Run((async () =>
                {
                    try
                    {
                        await testActor.PostAsync(new PostObject());
                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }
                }));
            }

            await Task.Delay(3000);
        }

        [Fact]
        public async void Cancel()
        {
            var testActor = new TestActor(new LimitedConcurrencyLevelTaskScheduler(1), _output);
            var cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    await testActor.PostAsync(new PostObject(), cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Debugger.Break();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
            await Task.Run((async () =>
            {
                await Task.Delay(100);
                cancellationTokenSource.Cancel();
            }));
            await Task.Delay(int.MaxValue);
        }

        [Fact]
        public void Post()
        {
            var testActor = new TestActor(new LimitedConcurrencyLevelTaskScheduler(1), _output);
            try
            {
                testActor.Post(new PostObject());
            }
            catch (Exception exception)
            {
                Debugger.Break();
                throw;
            }
        }

        [Fact]
        public void TestPath()
        {
            var log = "..\\Log";
            var applicationRoot = Extension.GetApplicationRoot();
            var fullPath = Path.GetFullPath(log, applicationRoot);
            var combine = Path.Combine(applicationRoot, log);
            combine = Path.GetFullPath(combine);
            Assert.Equal(fullPath, combine);
            _output.WriteLine(fullPath);
        }
    }

    public class PostObject
    {
    }

    public class TestActor : TaskBasedActor<PostObject>
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestActor(TaskScheduler scheduler, ITestOutputHelper outputHelper) : base(scheduler)
        {
            _outputHelper = outputHelper;
        }

        protected override async Task ReceiveMessage(PostObject message, CancellationToken token)
        {
            var newGuid = Guid.NewGuid();
            _outputHelper.WriteLine($"come into current thread {newGuid}");
            await Task.Delay(1000, token);
            _outputHelper.WriteLine($"leave current thread {newGuid}");
            throw new Exception("asdf");
        }
    }
}