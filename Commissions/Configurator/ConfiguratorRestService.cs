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
            setting.ModificationTime = TradeSetting.GetStringTime(DateTime.UtcNow);
            return _restService.Put<TradeSetting, TradeSetting>("/api/users/" + id + "/securitytradingsettings/" + setting.SecurityId, setting);
        }

        public Security[] GetSecurities()
        {
            return _restService.Get<Security[]>("/api/securities");
        }

        public Security SaveSecurity(string id, Security security)
        {
            security.ModificationTime = TradeSetting.GetStringTime(DateTime.UtcNow);
            return _restService.Put<Security, Security>("/api/securities/" + id, security);
        }
    }
}
