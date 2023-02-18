using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMate.Core
{
    public enum LogLevel
    {
        Error,
        Information
    }
    public class Logger
    {
        public static void Log(Exception exception)
        {
            if (CliHelper.LogEnabled)
                throw exception;
            else
                Log("error :" + exception.Message);
        }
        public static void Log(string message, LogLevel logLevel = LogLevel.Information)
        {
            if (logLevel == LogLevel.Information)
            {
                Console.WriteLine(message);
            }
            else
            {
                throw new Exception(message);
            }
        }

        internal static void CompleteTask()
        {
            Log("Command is Completed");
        }
    }
}
