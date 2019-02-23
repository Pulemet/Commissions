using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.CryptoCortex.Models;
using Commissions.Subscription;

namespace Commissions.CryptoCortex.Subscriptions
{
    public class SubscriptionFactory
    {
        protected const string ResponsesDestination = "/user/v1/responses";
        protected StompWebSocketService StompWebSocketService;
        private readonly string _url;
        private readonly string _token;

        public SubscriptionFactory(string url, string token)
        {
            _url = url;
            _token = token;
            StompWebSocketService = new StompWebSocketService(_url, _token);
        }

        public void ResponsesSubscribe(Action<string> action)
        {
            StompWebSocketService.Subscribe(ResponsesDestination, action);
        }

        public void StopResponses(Action<string> action)
        {
            StompWebSocketService.Unsubscribe(ResponsesDestination, action);
        }

        public void Close()
        {
            StompWebSocketService.Close();
        }
    }
}
