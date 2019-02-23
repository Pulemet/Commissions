using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commissions.Rest
{
    internal interface IRestService
    {
        void Authorize(string name, string password);
        string Token { get; }
        T Get<T>(string requestUrl);
        T Delete<T>(string requestUrl);
        T Post<T>(string requestUrl);
        Tout Post<Tin, Tout>(string requestUrl, Tin body);
        Tout Put<Tin, Tout>(string requestUrl, Tin body);
    }
}
