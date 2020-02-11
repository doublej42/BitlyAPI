using System.Collections.Generic;

namespace BitlyAPI
{
    public class BitlyGroup
    {
        //public References references { get; set; }
        public string Name { get; set; }
        public List<string> Bsds { get; set; }
        public string Created { get; set; }
        public bool IsActive { get; set; }
        public string Modified { get; set; }
        public string OrganizationGuid { get; set; }
        public string Role { get; set; }
        public string Guid { get; set; }
    }
}
