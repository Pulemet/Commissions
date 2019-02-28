using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.Configurator.Models
{
    public class Security
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("tickSize")]
        public double TickSize { get; set; }

        [JsonProperty("multiplier")]
        public string Multiplier { get; set; }

        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("termCurrency")]
        public string TermCurrency { get; set; }

        [JsonProperty("securityType")]
        public string SecurityType { get; set; }

        [JsonProperty("isSuspendedForTrading")]
        public bool IsSuspendedForTrading { get; set; }

        [JsonProperty("calendarCodeId")]
        public string CalendarCodeId { get; set; }

        [JsonProperty("isPopular")]
        public bool IsPopular { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("expectedSpread")]
        public string ExpectedSpread { get; set; }

        [JsonProperty("expectedDelay")]
        public string ExpectedDelay { get; set; }

        [JsonProperty("isQuotedHere")]
        public bool IsQuotedHere { get; set; }

        [JsonProperty("isAlive")]
        public bool IsAlive { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("creationTime")]
        public string CreationTime { get; set; }

        [JsonProperty("ModificationTime")]
        public string ModificationTime { get; set; }
    }
}
