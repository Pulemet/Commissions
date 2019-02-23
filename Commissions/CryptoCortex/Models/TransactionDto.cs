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
    public enum Type
    {
        profit_loss,
        trading_commission
    }
    public class TransactionDto
    {
        internal class EnumConverter : StringEnumConverter
        {
            public EnumConverter() : base(new SnakeCaseNamingStrategy(), false)
            {
            }
        }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter))]
        public Type Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("confirmations")]
        public string Confirmations { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("creation_time")]
        public long CreationTime { get; set; }

        [JsonProperty("post_balance")]
        public double PostBalance { get; set; }

        [JsonProperty("internal_transaction_id")]
        public string InternalTransactionId { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("confirmations_needed")]
        public string ConfirmationsNeeded { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("modification_time")]
        public long ModificationTime { get; set; }
    }
}
