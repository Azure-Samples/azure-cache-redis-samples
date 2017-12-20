using System;
using System.Globalization;
using System.IO;

namespace DotNet.ClientSamples.StackExchange.Redis
{
    internal static class LogUtility
    {
        public enum LogLevel
        {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3
        }

        public static TextWriter Logger { get; set; }
        public static LogLevel Level { get; set; } = LogLevel.Error;

        public static void LogWarning(string msg, params object[] args)
        {
            if (Level >= LogLevel.Warning)
            {
                Log(LogLevel.Warning, msg, args);
            }
        }

        public static void LogError(string msg, params object[] args)
        {
            if (Level >= LogLevel.Error)
            {
                Log(LogLevel.Error, msg, args);
            }
        }

        public static void LogInfo(string msg, params object[] args)
        {
            if (Level >= LogLevel.Info)
            {
                Log(LogLevel.Info, msg, args);
            }
        }

        public static void LogDebug(string msg, params object[] args)
        {
            if (Level >= LogLevel.Debug)
            {
                Log(LogLevel.Debug, msg, args);
            }
        }

        private static void Log(LogLevel level, string msg, params object[] args)
        {
            if (Logger != null)
            {
                string msgToPrint = (args.Length > 0) ? string.Format(msg, args) : msg;
                Logger.WriteLine("[{0}] - [{1}] - {2}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), level, msgToPrint);
            }
        }
    } 
}
