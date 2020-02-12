using System;
using System.Collections.Generic;

namespace BitlyAPI
{
    public class BitlyGroup
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public List<string> Bsds { get; set; }
        public string Guid { get; set; }
        public string OrganizationGuid { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
        public Dictionary<string,string> References { get; set; }
    }
}
