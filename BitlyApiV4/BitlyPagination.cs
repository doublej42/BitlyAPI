using System;
using System.Collections.Generic;
using System.Text;

namespace BitlyAPI
{
    public class BitlyPagination
    {
        public int Total { get; set; }
        public int Size { get; set; }
        public string Prev { get; set; }
        public int Page { get; set; }
        public string Next { get; set; }
    }
}
