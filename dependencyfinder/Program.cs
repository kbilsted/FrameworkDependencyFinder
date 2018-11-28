using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependencyFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var ignorePaths = new[] {
            };
            var basePath = @"c:\src";

            var info = GetVersions(basePath, ignorePaths);

            PrintStats(info);

            Console.ReadKey();
        }

        private static void PrintStats(ILookup<string, string> info)
        {
            foreach (var group in info.OrderBy(x => x.Key))
            {
                Console.WriteLine($"\n{@group.Key}");
                Console.WriteLine(string.Join("", Enumerable.Repeat("-", group.Key.Length)));

                int i = 1;
                foreach (var path in info[@group.Key])
                {
                    var lower = path.ToLower();
                    if (lower.EndsWith(".tests"))
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (lower.EndsWith(".client"))
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    if(lower.EndsWith(".domain"))
                        Console.ForegroundColor = ConsoleColor.DarkCyan;

                    Console.WriteLine($"{i++,3} {path}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                var subgroup = info[@group.Key].ToLookup(x => Path.GetExtension(x), x => x);
                var testCount = subgroup[".Tests"].Count();
                var clientCount = subgroup[".Client"].Count();
                var domainCount = subgroup[".Domain"].Count();
                Console.WriteLine("Statistics:");
                Console.WriteLine($"{"Tests:",-8} {testCount,-5}");
                Console.WriteLine($"{"Client:",-8} {clientCount,-5}");
                Console.WriteLine($"{"Domain:",-8} {domainCount,-5}");
                Console.WriteLine($"{"Total:",-8} {info[@group.Key].Count(),-5}");
                Console.WriteLine($"{"Easy:",-8} {testCount+clientCount+domainCount,-5}");
            }
        }

        private static ILookup<string, string> GetVersions(string basePath, string[] ignorePaths)
        {
            Regex targetFrameworkRegex = new Regex("<TargetFrameworkVersion>(?<version>v\\d+(\\.\\d+)*)", RegexOptions.Compiled);

            var info = Directory
                .EnumerateFiles(basePath, "*.csproj", SearchOption.AllDirectories)
                .Where(x => !ignorePaths.Any(x.ToLower().StartsWith))
                .Select(x =>
                {
                    var m = targetFrameworkRegex.Match(File.ReadAllText(x));
                    string version = m.Success
                        ? m.Groups["version"].Value
                        : "unknown";
                    return Tuple.Create(Path.GetDirectoryName(x), version);
                })
                .ToLookup(x => x.Item2, x => x.Item1);
            return info;
        }
    }
}

