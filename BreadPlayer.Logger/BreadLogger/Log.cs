using System;
using System.Collections.Generic;
using System.Linq;
using XLog.Formatters;
using XLog;
using BreadPlayer.Logger.Targets;

public class Log
{
    static Logger log;
    private static Logger Logger
    {
        get
        {
            return log;
        }
        set { log = value; }
    }
    public static void InitLogger(string logFilePath)
    {
        var formatter = new LineFormatter();
        var logConfig = new LogConfig(formatter);
        logConfig.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FastFileTarget(formatter, logFilePath));
        LogManager.Init(logConfig);
        Logger = LogManager.Default.GetLogger("BPLogger");
    }

    static int GetExceptionCount(string text)
    {
        char[] delimiters = new char[] { ' ', '\r', '\n', '|', ',', '.' };
        Dictionary<string, int> count =
     text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
     .GroupBy(s => s)
     .ToDictionary(g => g.Key, g => g.Count());
        int errorCount = 0;
        if (count.ContainsKey("Error".ToUpper()))
            errorCount += count["Error".ToUpper()];
        if (count.ContainsKey("Fatal".ToUpper()))
            errorCount += count["Fatal".ToUpper()];
        return errorCount;
    }

    public void F(string msg, Exception ex = null)
    {
        Logger.Fatal(msg, ex);
    }
    public void I(string msg, Exception ex = null)
    {
        Logger.Info(msg, ex);
    }
    public void D(string msg, Exception ex = null)
    {
        Logger.Debug(msg, ex);
    }
    public void E(string msg, Exception ex = null)
    {
        Logger.Error(msg, ex);
    }
}

