using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.CryptoCortex.Models;

namespace Commissions.CryptoCortex.Subscriptions
{
    public class ManagerSubscription : SubscriptionFactory
    {
        private const string TransactionsDestination = "/app/v1/transactions/complete";

        public ManagerSubscription(string url, string token) : base(url, token)
        {

        }

        public void GetTransactions(Action<string> action, long startTime)
        {
            StompWebSocketService.SendMessage("", TransactionsDestination, new TransactionsStartTime() {StartTime = startTime}, action);
        }
    }
}
