using NLog;
using NLog.Config;
using NLog.Targets;

namespace ComfySharp;


internal class LoggingManager {
    public LoggingConfiguration Config { get; private set; } = new();

    public LoggingManager() {
        var consoleTarget = new ColoredConsoleTarget("console") {
            Layout = @"${processtime}|${level}|${message}"
        };
#if DEBUG
        Config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
#elif RELEASE
        Config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
#endif
        
        LogManager.Configuration = Config;
    }
}

    public enum ELogLevel {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    public static class LoggingExtension {
        public static LogLevel ToNLogLevel(this ELogLevel level) {
            return level switch {
                ELogLevel.TRACE => LogLevel.Trace,
                ELogLevel.DEBUG => LogLevel.Debug,
                ELogLevel.INFO => LogLevel.Info,
                ELogLevel.WARN => LogLevel.Warn,
                ELogLevel.ERROR => LogLevel.Error,
                ELogLevel.FATAL => LogLevel.Fatal,
                _ => LogLevel.Off,
            };
        }
    }

    public static class Logger {
        static readonly private NLog.Logger ClassLogger = LogManager.GetCurrentClassLogger();

        public static void Trace(string message) {
            ClassLogger.Trace(message);
        }

        public static void Debug(string message) {
            ClassLogger.Debug(message);
        }

        public static void Info(string message) {
            ClassLogger.Info(message);
        }

        public static void Warn(string message) {
            ClassLogger.Warn(message);
        }

        public static void Error(string message) {
            ClassLogger.Error(message);
        }

        public static void Fatal(string message) {
            ClassLogger.Fatal(message);
        }

        public static void Log(ELogLevel level, string message) {
            ClassLogger.Log(level.ToNLogLevel(), message);
        }
    }