using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BinaryDetection
{
    public class BinaryDriver
    {
        public static void Main(string[] args)
        {
            Config.Initialize(args);
            DateTime startTime = DateTime.Now;

            Config.Instance.Log.LogInformation($"Starting binary detection, validation, and removal tool from {Environment.CurrentDirectory}...");

            DetectBinaries.Execute();
            CompareBinariesAgainstBaseline.Execute();
            RemoveBinaries.Execute();

            Config.Instance.Log.LogInformation("Finished binary detection, validation, and removal tool. Took " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
        }
    }
}
