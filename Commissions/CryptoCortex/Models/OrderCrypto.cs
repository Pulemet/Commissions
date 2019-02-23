using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Commissions.CryptoCortex.Models
{
    public enum OrderType
    {
        MARKET,
        LIMIT
    }

    public enum Side
    {
        BUY,
        SELL
    }

    public class OrderCrypto
    {
        internal class EnumConverter : StringEnumConverter
        {
            public EnumConverter() : base(new SnakeCaseNamingStrategy(), false)
            {
            }
        }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(EnumConverter))]
        public Side Side { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter))]
        public OrderType Type { get; set; }

        [JsonProperty("security_id")]
        public string SecurityId { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }
    }
}
