using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using CommandLine;

namespace EventsProducer
{
    partial class Program
    {
        private static Dictionary<string, EventData> InputFiles = new Dictionary<string, EventData>();
        private static Options options = null;
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        private static void StartMonitorFolder(string path)
        {
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("'EventModels' Directory files changed/updated, reloading...");
            InputFiles = GetInputFiles(options);
        }

        private static async Task SenderAsync()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(ConfigurationHelper.EventHubConnectionString, ConfigurationHelper.EventHubName);

            Console.WriteLine("Press any key to stop sending files to hub.");
            while (!Console.KeyAvailable)
            {
                foreach (var file in InputFiles)
                {
                    await eventHubClient.SendAsync(file.Value);
                    Console.WriteLine($"[{DateTime.UtcNow}] Input file '{file.Key}' has been sent!");
                    Thread.Sleep(options.Delay);
                }
            }

            eventHubClient.Close();
        }

        private static Dictionary<string, EventData> GetInputFiles(Options options)
        {
            var inputFiles = new Dictionary<string, EventData>();
            if (options.InputFiles == null || !options.InputFiles.Any())
            {
                return inputFiles;
            }

            foreach(var path in options.InputFiles)
            {
                if (File.Exists(path)) inputFiles.Add(path, new EventData(File.ReadAllBytes(path)));
                if (Directory.Exists(path))
                {
                    foreach (var folderPath in Directory.GetFiles(path))
                    {
                        inputFiles.Add(folderPath, new EventData(File.ReadAllBytes(folderPath)));
                    }
                }
            }
            return inputFiles;
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed((Options opts) =>
            {
                options = opts;
                StartMonitorFolder("EventModels");
                InputFiles = GetInputFiles(options);
                SenderAsync().GetAwaiter().GetResult();
            });
        }
    }
}
