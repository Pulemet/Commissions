using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.CryptoCortex.Models
{
    public class BalanceDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("available_for_withdrawal")]
        public double? AvailableForWithdrawal { get; set; }

        [JsonProperty("balance")]
        public double? Balance { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("available_for_trading")]
        public double? AvailableForTrading { get; set; }
    }
}
