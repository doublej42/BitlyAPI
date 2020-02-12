using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BitlyAPI
{
    public class BitlyShortenResponse
    {
        public bool Archived { get; set; }
        public List<string> Tags { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        public string Title { get; set; }
        public List<BitlyDeepLink> Deeplinks { get; set; }
        public string CreatedBy { get; set; }
        [JsonProperty("long_url")]
        public string LongUrl { get; set; }
        public string ClientId { get; set; }
        public List<string> CustomBitlinks { get; set; }
        public string Link { get; set; }
        public string Id { get; set; }

        public Dictionary<string, string> References { get; set; }
    }
}
