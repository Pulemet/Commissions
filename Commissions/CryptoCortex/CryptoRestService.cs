using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.Rest;

namespace Commissions.CryptoCortex
{
    public class CryptoRestService
    {
        private readonly IRestService _restService;

        internal CryptoRestService(IRestService restService)
        {
            _restService = restService;
        }

        public void Authorize(string user, string password)
        {
            _restService.Authorize(user, password);
        }

        public string GetToken => _restService.Token;
    }
}
