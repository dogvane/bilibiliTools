using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace µ¯Ä»ºÏ²¢.Common
{
    public static class Logger
    {
        static LoggerFactory factory = new LoggerFactory();

        static ILogger baseLogger = null;
        static Logger()
        {
            factory.AddConsole();
            baseLogger = factory.CreateLogger("");
        }

        public static void Info(string msg)
        {
            baseLogger.LogInformation(msg);
        }

        public static void Info(string format, params object[]  objs)
        {
            baseLogger.LogInformation(format, objs);
        }

        public static void Warn(string format)
        {
            baseLogger.LogWarning(format);
        }

        public static void Warn(string format, params object[] objs)
        {
            baseLogger.LogWarning(format, objs);
        }

        public static void Error(string format)
        {
            baseLogger.LogError(format);
        }

        public static void Error(Exception ex, string format, params object[] objs)
        {
            baseLogger.LogError(ex, format, objs);
        }

        public static void Error(string format, params object[] objs)
        {
            baseLogger.LogError(format, objs);
        }
    }
}
