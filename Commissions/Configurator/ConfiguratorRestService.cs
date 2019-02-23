using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.Configurator.Models;
using Commissions.Rest;
using Newtonsoft.Json;

namespace Commissions.Configurator
{ 
    public class ConfiguratorRestService
    {
        private readonly IRestService _restService;

        internal ConfiguratorRestService(IRestService restService)
        {
            _restService = restService;
        }

        public void Authorize(string user, string password)
        {
            _restService.Authorize(user, password);
        }

        public UserDto[] SearchUser(UserForSearch user)
        {
            return _restService.Post<UserForSearch, UserDto[]>("/api/users/search", user);
        }

        public FullTradeSettings[] GeTradeSettings(string id)
        {
            return _restService.Get<FullTradeSettings[]>("/api/users/" + id + "/securitytradingsettings");
        }

        public TradeSetting SaveTradeSetting(string id, TradeSetting setting)
        {
            return _restService.Put<TradeSetting, TradeSetting>("/api/users/" + id + "/securitytradingsettings/" + setting.SecurityId, setting);
        }
    }
}
