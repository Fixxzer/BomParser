using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OutputAnalyzer
{
    internal static class Program
    {
        private static void Main()
        {
            // Assumptions:
            // 1. epub has been unzipped
            // 2. BomEpubParser has parsed and stripped all the xml out of the files
            // 3. Files to process are located at the hard coded location below

            var root = "C:\\Users\\jacob.garner\\Downloads\\_testCode\\bomOutput";

            StringBuilder sb = new StringBuilder();
            foreach (var dir in Directory.GetDirectories(root))
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                foreach (var file in Directory.GetFiles(dir))
                {
                    string allText = File.ReadAllText(file);
                    string[] allWords = allText.Split(' ');

                    foreach (var word in allWords)
                    {
                        if (!dict.ContainsKey(word))
                        {
                            dict.Add(word, 0);
                        }

                        dict[word]++;
                    }
                }

                Console.WriteLine(Path.GetFileName(dir));
                Console.WriteLine("-------------------");

                sb.Append(Path.GetFileName(dir));

                foreach (var orderedItem in dict.OrderByDescending(pair => pair.Value).Take(25))
                {
                    Console.WriteLine($"{orderedItem.Key}: {orderedItem.Value}");
                    sb.AppendLine($",{orderedItem.Key},{orderedItem.Value}");
                }

                Console.WriteLine("-------------------");
                //File.WriteAllText($"{root}\\output.csv", sb.ToString());
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
