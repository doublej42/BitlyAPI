using System;
using System.Collections.Generic;
using System.Text;

namespace BitlyAPI
{
    public class BitlyBitlinksResponse
    {
        public BitlyPagination Pagination { get; set; }
        public List<BitlyLink> Links { get; set; }
    }
}
