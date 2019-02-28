using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.CryptoCortex.Models.Orders
{
    public class OrderResponce
    {
        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("events")]
        public Event[] Events { get; set; }
    }
}
