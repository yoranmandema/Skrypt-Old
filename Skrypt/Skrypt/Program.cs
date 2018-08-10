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
            engine.SetValue("WriteLine", new Action<object>(Console.WriteLine));
            engine.Parse(code);

            Console.ReadKey();
        }
    }
}