using System.Collections.Generic;

namespace Acmebot.Provider.Infoblox.Models
{
    public class ZoneResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> NameServers { get; set; }
    }
}