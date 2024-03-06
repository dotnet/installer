// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryToolKit;

public static class Log
{
    private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(ConfigureLogger);

    public static void LogDebug(string message)
    {
        _logger.Value.LogDebug(message);
    }

    public static void LogInformation(string message)
    {
        _logger.Value.LogInformation(message);
    }

    public static void LogWarning(string message)
    {
        _logger.Value.LogWarning(message);
    }

    public static void LogError(string message)
    {
        _logger.Value.LogError(message);
    }

    private static ILogger ConfigureLogger()
    {
        var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
        LogLevel level = Enum.TryParse<LogLevel>(logLevel, out var parsedLevel) ? parsedLevel : LogLevel.Information;

        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.UseUtcTimestamp = true;
                })
                .SetMinimumLevel(level));
        return loggerFactory.CreateLogger("BinaryTool");
    }
}