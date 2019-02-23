using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Subscription;

namespace Commissions.Subscriptions
{
    public class TraderSubscription : SubscriptionFactory
    {
        private const string OrdersDestination = "/app/v1/orders/create";
        private const string BalanceDestination = "/app/v1/accounts";

        public TraderSubscription(string url, string token) : base(url, token)
        {

        }

        public void SendOrder(OrderCrypto order, Action<string> action)
        {
            StompWebSocketService.SendMessage(OrdersDestination, order, action);
        }

        public void CheckBalance(Action<string> action)
        {
            StompWebSocketService.SendMessage(BalanceDestination, "", action);
        }
    }
}
