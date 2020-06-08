using System;
using System.Collections.Generic;
using System.Text;

namespace BomEpubParser
{
    public class ChapterModel
    {
        public string ChapterName { get; set; }
        public List<string> VerseList { get; set; }

        public ChapterModel()
        {
            VerseList = new List<string>();
        }
    }
}
