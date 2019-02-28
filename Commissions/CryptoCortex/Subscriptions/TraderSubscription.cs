using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Models.Orders;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Subscription;

namespace Commissions.Subscriptions
{
    public class TraderSubscription : SubscriptionFactory
    {
        private const string CreateOrder = "/app/v1/orders/create";
        private const string BalanceDestination = "/app/v1/accounts";
        private const string OrdersSubscribe = "/user/v1/orders";
        private const string DeleteOrder = "/app/v1/orders/cancel";

        public TraderSubscription(string url, string token) : base(url, token)
        {

        }

        public void SendOrder(OrderCrypto order, Action<string> action)
        {
            StompWebSocketService.SendMessage("", CreateOrder, order, action);
        }

        public void CheckBalance(Action<string> action)
        {
            StompWebSocketService.SendMessage("", BalanceDestination, "", action);
        }

        public void CancelOrder(string orderId, Action<string> action)
        {
            string idHeader = String.Format("X-Deltix-Order-ID:{0}\r\n", orderId);
            StompWebSocketService.SendMessage(idHeader, DeleteOrder, "", action);
        }

        public void OrderBookSubcribe(string topic, Action<string> action)
        {
            StompWebSocketService.Subscribe(topic, action);
        }

        public void OrdersReceiver(Action<string> action)
        {
            StompWebSocketService.Subscribe(OrdersSubscribe, action);
        }

        public void OrderBookUnsubscribe(string topic, Action<string> action)
        {
            StompWebSocketService.Unsubscribe(topic, action);
        }

        public void OrdersUnsubscribe(Action<string> action)
        {
            StompWebSocketService.Unsubscribe(OrdersSubscribe, action);
        }
    }
}
