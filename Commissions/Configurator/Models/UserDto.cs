using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.Configurator.Models
{
    public class UserDto
    {
        [JsonProperty("userId")]
        public String UserId { get; set; }

        [JsonProperty("brokerId")]
        public String BrokerId { get; set; }

        [JsonProperty("businessUnitId")]
        public String BusinessUnitId { get; set; }

        [JsonProperty("userGroupId")]
        public String UserGroupId { get; set; }

        [JsonProperty("tradingSettingsId")]
        public String TradingSettingsId { get; set; }

        [JsonProperty("externalId")]
        public String ExternalId { get; set; }

        [JsonProperty("profile")]
        public String Profile { get; set; }

        [JsonProperty("kYCStatus")]
        public String KYCStatus { get; set; }

        [JsonProperty("username")]
        public String Username { get; set; }

        [JsonProperty("isActivated")]
        public bool IsActivated { get; set; }

        [JsonProperty("isAlive")]
        public bool IsAlive { get; set; }

        [JsonProperty("version")]
        public String Version { get; set; }

        [JsonProperty("creationTime")]
        public String CreationTime { get; set; }

        [JsonProperty("ModificationTime")]
        public String ModificationTime { get; set; }
    }
}
