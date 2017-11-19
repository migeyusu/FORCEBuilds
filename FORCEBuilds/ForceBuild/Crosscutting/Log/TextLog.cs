using System;
using System.IO;

namespace FORCEBuild.Crosscutting.Log
{
    public class TextLog:ILog
    {
        public string Path { get; set; }
        private StreamWriter writer;
        public TextLog(string path)
        {
            Path = path;
        }
        public void Write(string sentence)
        {
            writer.WriteLine($"[{DateTime.Now}] {sentence}");
        }

        public void Write(Exception ex)
        {
            while (true)
            {
                writer.WriteLine($"[{DateTime.Now}] 异常类型：{ex.GetType()}\r\n异常消息：{ex.Message}\r\n异常信息：{ex.StackTrace} \r\n ");
                if (ex.InnerException != null)
                {
                    writer.WriteLine("Inner Exception: \r\n");
                    ex = ex.InnerException;
                    continue;
                }
                break;
            }
        }

        public void Open()
        {
            writer = new StreamWriter(Path, true) {AutoFlush = true};
        }
        public void Close()
        {
            writer.Close();
        }
    }
}
