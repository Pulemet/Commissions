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

        public void ResponsesSubscribe()
        {
            StompWebSocketService.Subscribe(ResponsesDestination, CheckWebSocketStatus);
        }

        protected void CheckWebSocketStatus(string message)
        {
            if (message.Length > 3 && message.Substring(0, 3) != "200")
            {
                Console.WriteLine("Error! Status: {0}", message.Substring(0, 3));
            }
        }

        public void StopResponses()
        {
            StompWebSocketService.Unsubscribe(ResponsesDestination, CheckWebSocketStatus);
        }

        public void Close()
        {
            StompWebSocketService.Close();
        }
    }
}
