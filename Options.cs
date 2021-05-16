using CommandLine;
using System.Collections.Generic;

namespace EventsProducer
{
    partial class Program
    {
        class Options
        {
            [Option('f', "files", Default = new[] { "EventModels" } ,HelpText = "Input files or Directory to be published.")]
            public IList<string> InputFiles { get; set; }

            [Option('d', "delay", Default = 1000, HelpText = "Delay in milliseconds before repeat of publishing file sequence.")]
            public int Delay { get; set; }
        }
    }
}
