using System;
using System.Text;

namespace FORCEBuild.Crosscutting
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// 面向用户的错误提示
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetCascadeMessage(this Exception exception)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Message:");
            var preException = exception;
            do
            {
                stringBuilder.AppendLine(preException.Message);
                preException = preException.InnerException;
                if (preException != null)
                {
                    stringBuilder.AppendLine("--------------Inner message---------------");
                }
            } while (preException != null);

            return stringBuilder.ToString();
        }

        public static string ToString(this Exception exception, StringBuilder stringBuilder)
        {
            stringBuilder.Append("Error:");
            var preException = exception;
            do
            {
                stringBuilder.AppendLine(preException.Message);
                stringBuilder.AppendLine(preException.StackTrace);
                preException = preException.InnerException;
                if (preException != null)
                {
                    stringBuilder.Append("Inner Exception:");
                }
            } while (preException != null);

            return stringBuilder.ToString();
        }

        public static string ToStringEx(this Exception exception) => exception.ToString(new StringBuilder());
    }
}