using System;
using System.Threading;
using System.Threading.Tasks;
namespace FORCEBuild.Concurrency
{
    /// <summary>
    /// 代表一个标准的、可停止的后台进程
    /// </summary>
    public class BackgroundTask : IDisposable
    {
        protected readonly Func<CancellationToken, Task> Action;
        public const int Running = 1;
        public const int Sleep = 2;
        protected int Status = 0;

        public bool IsRunning
        {
            get { return Status == Running; }
        }

        public BackgroundTask(Func<CancellationToken, Task> action)
        {
            this.Action = action;
            this.Status = Sleep;
        }

        protected CancellationTokenSource TokenSource { get; set; }

        public Task IncludeTask { get; private set; }

        public async Task Start()
        {
            if (Interlocked.CompareExchange(ref Status, Running, Sleep) == Sleep)
            {
                using (TokenSource = new CancellationTokenSource())
                {
                    var tokenSourceToken = TokenSource.Token;
                    IncludeTask = Task.Run(async () =>
                    {
                        try
                        {
                            await Action.Invoke(this.TokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            //do nothing
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            Status = Sleep;
                        }
                    }, tokenSourceToken);
                    await IncludeTask;
                }
            }
        }

        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref Status, Running, Sleep) == Running)
            {
                TokenSource.Cancel();
                Task.WaitAll(IncludeTask);
            }
        }

        public virtual async Task StopAsync()
        {
            if (Interlocked.CompareExchange(ref Status, Running, Sleep) == Running)
            {
                TokenSource.Cancel();
                await Task.WhenAll(IncludeTask);
            }
        }

        public virtual void Dispose()
        {
            if (Status == Running)
            {
                Stop();
            }
        }
    }

    public class BackgroundTask<T> : IDisposable
    {
        protected readonly Func<CancellationToken, Task<T>> Func;
        protected int Status = 0;

        public BackgroundTask(Func<CancellationToken, Task<T>> func)
        {
            this.Func = func;
            this.Status = BackgroundTask.Sleep;
        }

        public bool IsRunning
        {
            get { return Status == BackgroundTask.Running; }
        }

        protected CancellationTokenSource TokenSource { get; set; }

        public Task<T> IncludeTask { get; private set; }

        public virtual async Task<T> Start()
        {
            if (Interlocked.CompareExchange(ref Status, BackgroundTask.Running, BackgroundTask.Sleep) ==
                BackgroundTask.Sleep)
            {
                using (this.TokenSource = new CancellationTokenSource())
                {
                    var tokenSourceToken = TokenSource.Token;
                    IncludeTask = Task.Run(async () =>
                    {
                        try
                        {
                            return await Func.Invoke(this.TokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return default;
                        }
                        finally
                        {
                            Status = BackgroundTask.Sleep;
                        }
                    }, tokenSourceToken);
                    return await IncludeTask;
                }
            }

            return default(T);
        }

        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref Status, BackgroundTask.Running, BackgroundTask.Sleep) ==
                BackgroundTask.Running)
            {
                TokenSource.Cancel();
                Task.WaitAll(IncludeTask);
            }
        }

        public virtual async Task StopAsync()
        {
            if (Interlocked.CompareExchange(ref Status, BackgroundTask.Running, BackgroundTask.Sleep) ==
                BackgroundTask.Running)
            {
                TokenSource.Cancel();
                await Task.WhenAll(IncludeTask);
            }
        }

        public virtual void Dispose()
        {
            if (Status == BackgroundTask.Running)
            {
                Stop();
            }
        }
    }
}