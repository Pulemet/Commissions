using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.Configurator.Models
{
    public class UserForSearch
    {
        [JsonProperty("name")]
        public String Name { get; set; }
    }
}
