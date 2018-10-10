using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Skrypt.Engine;

namespace Skrypt
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Get skrypt test code
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\SkryptFiles\testcode.skt");
            var pathInclude = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\SkryptFiles\include.skt");
            var code = File.ReadAllText(path);

            // Creating a skrypt engine object
            var engine = new SkryptEngine();

            //engine.Parse(code);
            engine.DoFile(pathInclude);

            double time = 0;
            int instances = 0;
            if (instances > 0) {
                for (int i = 0; i < instances; i++) {
                    var e = new SkryptEngine() {
                        Settings = EngineSettings.NoLogs
                    };
                    e.DoFile(pathInclude);

                    time += e.ExecutionTime;
                }

                Console.WriteLine($"Average ({instances} instances): {time / instances}ms");
                Console.WriteLine($"Per second: {1000 / (time / instances)}");
            }

            Console.ReadKey();
        }
    }
}