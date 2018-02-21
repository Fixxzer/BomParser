using System.IO;
using System.Text;
using HtmlAgilityPack;

namespace BomEpubParser
{
    internal static class Program
    {
        private static void Main()
        {
            // This assumes a couple of things
            // 1. epub has been unzipped to the location below

            var files = Directory.GetFiles("C:\\Users\\jacob.garner\\Downloads\\_testCode\\xml");

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                PrintChapter(text);
            }
        }

        private static void PrintChapter(string text)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);

            var bookNode = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='runHead']");
            if (bookNode != null)
            {
                var root = "C:\\Users\\jacob.garner\\Downloads\\_testCode\\bomOutput\\";
                var book = bookNode.InnerText;
                var chapter = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='titleNumber']").InnerText;

                if (!Directory.Exists($"{root}{book}"))
                {
                    Directory.CreateDirectory($"{root}{book}");
                }

                StringBuilder sb = new StringBuilder();
                var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//p[@class='verse-first'] | //p[@class='verse']");

                foreach (HtmlNode node in htmlNodeCollection)
                {
                    sb.AppendLine(node.InnerText.Trim());
                }

                string chapterText = sb.ToString();
                //Console.WriteLine(chapterText);
                File.AppendAllText($"{root}{book}\\{chapter}.txt", chapterText);
            }
        }
    }
}