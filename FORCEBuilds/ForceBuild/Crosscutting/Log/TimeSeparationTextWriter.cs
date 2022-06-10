using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using FORCEBuild.Concurrency;

namespace FORCEBuild.Crosscutting.Log
{
    /// <summary>
    /// 按时间划分的文件写入[Thread-Safety]
    /// </summary>
    public class TimeSeparationTextWriter : IDisposable
    {
        private readonly ConcurrentQueue<string> _collection = new ConcurrentQueue<string>();

        private BackgroundTask _backgroundTask;

        public TimeSeparationTextWriter(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            Start(folderPath);
        }

        public async void Start(string folderPath)
        {
            _backgroundTask = new BackgroundTask((async token =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_collection.Count > 0)
                    {
                        try
                        {
                            var preFileName = $"{DateTime.Now:yyyy-MM-dd}.txt";
#if NETFRAMEWORK
                            var filePath = Path.Combine(folderPath, preFileName);
                            filePath = Path.GetFullPath(filePath);
#else
                            var filePath = Path.GetFullPath(preFileName, folderPath);
#endif
                            using (var streamWriter = new StreamWriter(filePath, true))
                            {
                                while (_collection.TryDequeue(out var s))
                                {
                                    await streamWriter.WriteLineAsync(s);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //do nothing
                        }
                    }

                    await Task.Delay(500, token);
                }
            }));
            await _backgroundTask.Start();
        }


        public void Append(string file)
        {
            if (!_backgroundTask.IsRunning)
            {
                return;
            }

            _collection.Enqueue(file);
        }

        public void Dispose()
        {
            _backgroundTask.Dispose();
        }
    }
}