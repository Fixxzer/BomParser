using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace BomEpubParser
{
    internal static class Program
    {
        private static void Main()
        {
            const string path = "C:\\Users\\jacob.garner\\Downloads\\_bomparser\\";

            Console.WriteLine("Parsing data...");
            var books = ParseFiles(path + "bom");

            Console.WriteLine("Analyzing data...");
            var wordCounts = Analyze(books);
            Console.WriteLine("Analysis complete.");

            Console.WriteLine("Generating report...");
            GenerateReport(wordCounts, path);
            Console.WriteLine("Report complete.");

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static Dictionary<string, List<ChapterModel>> ParseFiles(string path)
        {
            // This assumes epub has been unzipped to the location below
            //      1. Download ePub
            //      2. Using an unzip utility, extract the ePub to a directory
            //      3. Grab all the .xhtml files and dump them to the folder specified below

            var files = Directory.GetFiles(path);
            Dictionary<string, List<ChapterModel>> bookList = new Dictionary<string, List<ChapterModel>>();

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                var chapterModelKvp = SaveChapterText(text);
                if (!string.IsNullOrEmpty(chapterModelKvp.Key))
                {
                    if (!bookList.ContainsKey(chapterModelKvp.Key))
                    {
                        // Insert
                        bookList.Add(chapterModelKvp.Key, new List<ChapterModel> {chapterModelKvp.Value});
                    }
                    else
                    {
                        // Update
                        bookList[chapterModelKvp.Key].Add(chapterModelKvp.Value);
                    }
                }
            }

            return bookList;
        }

        private static KeyValuePair<string, ChapterModel> SaveChapterText(string text)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);

            // Return if it's only a chapter heading
            if (htmlDocument.DocumentNode.SelectSingleNode("//div[@class='book-toc']") != null)
            {
                return new KeyValuePair<string, ChapterModel>();
            }

            var bookNode = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='runHead']")?.InnerText;
            var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='title']")?.Attributes?["title"]?.Value;

            var bookName = string.IsNullOrEmpty(bookNode) ? titleNode : bookNode;

            var chapter = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='titleNumber']")?.InnerText;
            if (chapter == null)
            {
                return new KeyValuePair<string, ChapterModel>();
            }

            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//p[@class='verse-first'] | //p[@class='verse']");

            List<string> verseList = new List<string>();
            foreach (HtmlNode node in htmlNodeCollection)
            {
                verseList.Add(node.InnerText.Trim());
            }

            return new KeyValuePair<string, ChapterModel>(bookName, new ChapterModel(){ChapterName = chapter, VerseList = verseList});
        }

        private static Dictionary<string, Dictionary<string, int>> Analyze(Dictionary<string, List<ChapterModel>> books)
        {
            // Build the data structure and count the words
            Dictionary<string, Dictionary<string, int>> wordsByBook = new Dictionary<string, Dictionary<string, int>>();
            foreach (KeyValuePair<string, List<ChapterModel>> book in books)
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                foreach (var chapterModel in book.Value)
                {
                    foreach (var verse in chapterModel.VerseList)
                    {
                        string[] allWords = verse.Split(' ');
                        foreach (var word in allWords)
                        {
                            var sanitizedWord = SanitizeWord(word);
                            if (!string.IsNullOrEmpty(sanitizedWord))
                            {
                                if (!dict.ContainsKey(sanitizedWord))
                                {
                                    dict.Add(sanitizedWord, 0);
                                }

                                dict[sanitizedWord]++;
                            }
                        }
                    }
                }

                wordsByBook.Add(book.Key, dict);
            }

            return wordsByBook;
        }

        private static string SanitizeWord(string word)
        {
            // Convert to lower case
            var wordToLower = word.ToLower();

            // remove non characters
            StringBuilder sanitizedWord = new StringBuilder();
            for (int i = 0; i < wordToLower.Length; i++)
            {
                if (wordToLower[i] >= 97 && wordToLower[i] <= 122)
                {
                    sanitizedWord.Append(wordToLower[i]);
                }
            }

            return sanitizedWord.ToString();
        }
        
        private static void GenerateReport(Dictionary<string, Dictionary<string, int>> wordCounts, string path)
        {
            const int maxRecords = 50;
            StringBuilder sb = new StringBuilder();
            foreach (var book in wordCounts)
            {
                sb.Append($"{book.Key},");
            }

            sb.AppendLine();

            for (int i = 0; i < maxRecords; i++)
            {
                foreach (var book in wordCounts)
                {
                    foreach (var count in book.Value.OrderByDescending(pair => pair.Value).Skip(i).Take(1))
                    {
                        sb.Append($"{count.Key} ({count.Value}),");
                    }
                }

                sb.AppendLine();
            }

            //Console.WriteLine(sb.ToString());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText($"{path}\\output.csv", sb.ToString());
        }
    }
}