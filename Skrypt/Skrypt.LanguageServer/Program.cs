using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace SampleServer {
    class Program {
        static void Main(string[] args) {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args) {

        }
    }
}