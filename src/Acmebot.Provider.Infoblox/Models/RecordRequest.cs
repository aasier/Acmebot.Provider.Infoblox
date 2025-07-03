using System.Collections.Generic;

namespace Acmebot.Provider.Infoblox.Models
{
    public class RecordRequest
    {
        public string Type { get; set; }
        public int Ttl { get; set; }
        public List<string> Values { get; set; }
    }
}