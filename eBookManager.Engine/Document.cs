using System;
using System.Collections.Generic;
using System.Text;

namespace eBookManager.Engine
{
    public class Document
    {
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime Created { get; set; }
        public String FilePath { get; set; }
        public String FileSize { get; set; }
        public String ISBN { get; set; }
        public String Price { get; set; }
        public String Publisher { get; set; }
        public String Author { get; set; }
        public DateTime PublishDate { get; set; }
        public DeweyDecimal Classification { get; set; }
        public String Category { get; set; }
    }
}
