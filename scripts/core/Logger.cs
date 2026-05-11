using System;
using Godot;

public static class Logger
{
    public static void Log(LogLevel level, params object[] message)
    {
        var dateTime = DateTime.Now;
        var timestamp = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
        var callingMethod = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod();
        string logMessage = $"{timestamp} [{level}] [{callingMethod.DeclaringType.Name}] [{callingMethod.Name}] ";
        string color = level switch
        {
            LogLevel.DEBUG => "WHITE",
            LogLevel.INFO => "CYAN",
            LogLevel.WARNING => "YELLOW",
            LogLevel.ERROR => "MAROON",
            _ => "WHITE"
        };

        GD.PrintRich([$"[color={color}]{logMessage}[/color]", .. message]);
    }

    public static void Debug(params object[] message)
    {
        Log(LogLevel.DEBUG, message);
    }

    public static void Info(params object[] message)
    {
        Log(LogLevel.INFO, message);
    }

    public static void Warning(params object[] message)
    {
        Log(LogLevel.WARNING, message);
    }

    public static void Error(params object[] message)
    {
        Log(LogLevel.ERROR, message);
    }
}