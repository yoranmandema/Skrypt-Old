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
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"..\..\SkryptFiles\testcode.skt");
            var code = File.ReadAllText(path);

            // Creating a skrypt engine object
            var engine = new SkryptEngine();

            engine.AddClass(typeof(Vector));
            engine.Parse(code);

            //int frames = 1000;
            //var stopwatch = Stopwatch.StartNew();
            //for (int i = 0; i < frames; i++) {
            //    engine.Executor.Invoke("Update", 1);
            //}
            //stopwatch.Stop();
            //Console.WriteLine($"Invoke ({frames} frames): {stopwatch.Elapsed.TotalMilliseconds / frames}ms");

            Console.ReadKey();
        }
    }
}