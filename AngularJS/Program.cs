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
            CheckFeatureNameDuplicates("service", @"C:\pe\platform\src\WebApp\scripts\app", new[] { "ts" }, "*");
            CheckFeatureNameDuplicates("directive", @"C:\pe\platform\src\WebApp\scripts\app", new[] { "ts" }, "*");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }

        private static void CheckFeatureNameDuplicates(string featurePattern, string path, string[] fileTypes, string filePattern = "*")
        {
            PrintHeader($"Feature: {featurePattern} @ {path}");

            var files = new List<string>();
            var features = new List<(string File, string Line, string Name)>();

            var featureRegex = new Regex($"\\.{featurePattern}\\(['\"](.*)['\"]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var injectRegex = new Regex("\\$inject[^\\]]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            fileTypes
                .ToList()
                .ForEach(type => files.AddRange(Directory.GetFiles(path, $"{filePattern}.{type}", SearchOption.AllDirectories)));

            foreach (var file in files)
            {
                var lines = File.ReadLines(file);

                foreach (var line in lines)
                {
                    var matches = featureRegex.Matches(line);

                    if (matches.Any())
                    {
                        var name = matches[0].Groups[1].Value;

                        features.Add((file, line, name));
                    }
                }
            }

            var totalDiscovered = 0;

            foreach (var feature in features)
            {
                var duplicates = features.Where(s => s.Name == feature.Name);

                var duplicateCount = duplicates.Count();

                if (duplicateCount == 1)
                    continue;

                totalDiscovered++;

                PrintHeader($"{feature.Name}: {duplicateCount}");

                foreach (var duplicate in duplicates)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{duplicate.File}");
                    Console.WriteLine($"    {duplicate.Line}");
                }

                var matches = new List<(string File, string Injects)>();

                foreach (var file in files)
                {
                    var text = File.ReadAllText(file);

                    var injectMatches = injectRegex.Matches(text);

                    if (!injectMatches.Any())
                        continue;

                    var injects = injectMatches[0].Value;

                    if (Regex.IsMatch(injects, $"['\"]{feature.Name}['\"]", RegexOptions.IgnoreCase))
                    {
                        matches.Add((file, injects));
                    }
                }

                Console.WriteLine("");
                Console.WriteLine($"Matches: {matches.Count()}");

                foreach (var match in matches)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{match.File}");
                    Console.WriteLine(match.Injects);
                }
            }

            PrintHeader($"Total discovered: {totalDiscovered}");
        }

        private static void PrintHeader(string header)
        {
            Console.WriteLine("");
            Console.WriteLine($"==================================================================================================");
            Console.WriteLine($"{header}");
            Console.WriteLine($"==================================================================================================");
        }
    }
}
