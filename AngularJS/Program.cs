using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AngularJS
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckServiceNameDuplicates(@"C:\pe\platform\src\WebApp\scripts\app", new[] { "ts" }, "*");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }

        private static void CheckServiceNameDuplicates(string path, string[] fileTypes, string filePattern = "*")
        {
            var files = new List<string>();
            var services = new List<(string File, string Line, string Name)>();
            var serviceRegex = new Regex("\\.service\\(['\"](.*)['\"]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            fileTypes
                .ToList()
                .ForEach(type => files.AddRange(Directory.GetFiles(path, $"{filePattern}.{type}", SearchOption.AllDirectories)));

            foreach (var file in files)
            {
                var lines = File.ReadLines(file);

                foreach (var line in lines)
                {
                    var matches = serviceRegex.Matches(line);

                    if (matches.Any())
                    {
                        var name = matches[0].Groups[1].Value;

                        services.Add((file, line, name));

                        //Console.WriteLine($"{file}");
                        //Console.WriteLine($"    {line}");
                        //Console.WriteLine($"    {name}");
                    }
                }
            }

            foreach (var service in services)
            {
                var duplicates = services.Where(s => s.Name == service.Name);

                var duplicateCount = duplicates.Count();

                if (duplicateCount == 1)
                    continue;

                Console.WriteLine($"{service.Name}: {duplicateCount}");

                foreach (var duplicate in duplicates)
                {
                    Console.WriteLine($"{duplicate.File}");
                    Console.WriteLine($"    {duplicate.Line}");
                }
            }
        }
    }
}
