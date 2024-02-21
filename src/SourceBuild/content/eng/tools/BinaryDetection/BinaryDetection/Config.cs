using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace BinaryDetection
{
    public sealed class Config
    {
        // Singleton instance
        private static Config instance = default!;

        // Command line arguments
        public string RootDir { get; private set; } = string.Empty;
        public string BinaryBaselineFile { get; private set; } = string.Empty;
        
        // Output report files
        public string DetectedBinariesFile { get; private set; } = String.Empty;
        public string UpdatedBinaryBaselineFile { get; private set; } = String.Empty;
        public string NewBinariesFile { get; private set; } = String.Empty;

        // Logger
        public ILogger Log { get; private set; } = default!;

        // Private constructor to prevent instantiation
        private Config(string[] args)
        {
            SetupLogger();
            SetupConfiguration(args);
        }

        // Setup logger with console output
        private void SetupLogger()
        {
            // Log = new LoggerFactory().AddConsole().CreateLogger("BinaryTooling");
            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                        options.UseUtcTimestamp = true;
                    }));

            Log = loggerFactory.CreateLogger("BinaryTooling");
        }

        // Setup configuration from command line arguments
        private void SetupConfiguration(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Log.LogError("Invalid number of arguments. Usage: BinaryDetection <rootDir> <outputReportDir> optional:<binaryBaselineFile>");
                Environment.Exit(1);
            }

            // Set RootDir property
            RootDir = Path.GetFullPath(args[0]);
            if(!Directory.Exists(RootDir))
            {
                Log.LogError($"Root directory {RootDir} does not exist.");
                Environment.Exit(1);
            }

            // Set report file properties
            string outputReportDir = Path.GetFullPath(args[1]);
            if(!Directory.Exists(outputReportDir))
            {
                Directory.CreateDirectory(outputReportDir);
            }
            DetectedBinariesFile = Path.Combine(outputReportDir, "DetectedBinaries.txt");
            UpdatedBinaryBaselineFile = Path.Combine(outputReportDir, "UpdatedBinaryBaselineFile.txt");
            NewBinariesFile = Path.Combine(outputReportDir, "NewBinaries.txt");

            // Set BinaryBaselineFile property
            if (args.Length == 3)
            {
                BinaryBaselineFile = args[2];
                if (!File.Exists(BinaryBaselineFile))
                {
                    Log.LogError($"Allowed binaries file {BinaryBaselineFile} does not exist.");
                    Environment.Exit(1);
                }
            }
        }

        // Singleton instance
        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new Exception("Config instance has not been initialized. Call Initialize first.");
                }
                return instance;
            }
        }

        // Initialize singleton instance
        public static void Initialize(string[] args)
        {
            if (instance != null)
            {
                throw new Exception("Config instance has already been initialized.");
            }
            instance = new Config(args);
        }
    }
}