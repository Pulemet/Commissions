using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commissions.CryptoCortex;
using Commissions.CryptoCortex.Models.Orders;
using Commissions.Rest;
using Commissions.Subscriptions;

namespace Commissions.Tests
{
    public class TestTrader
    {
        private RestService CryptoRest { get; set; }
        private CryptoRestService CryptoService { get; set; }
        private TraderSubscription TraderSubscriber { get; set; }
        public void Initialize()
        {
            CryptoRest = new RestService(Constants.CryptoUrl, "/oauth/token", Constants.CryptoAuthorization);
            CryptoService = new CryptoRestService(CryptoRest);
            CryptoService.Authorize(Constants.CryptoTraderName, Constants.CryptoUserPass);
            // Trader subscription
            TraderSubscriber = new TraderSubscription(Constants.CryptoTraderSubscribe, CryptoService.GetToken);
            Thread.Sleep(500);
            TraderSubscriber.ResponsesSubscribe();
        }

        public void ExecuteOrder(OrderCrypto order)
        {
            OrderCrypto tradeOrder = (OrderCrypto)order.Clone();
            tradeOrder.Side = order.Side == Side.BUY ? Side.SELL : Side.BUY;
            TraderSubscriber.SendOrder(tradeOrder, OrderResponce);
        }

        private void OrderResponce(string message)
        {
            if (message.Substring(0, 3) != "200")
            {
                throw new Exception(String.Format("Trader could not send order, Status: {1}", message, message.Substring(0, 3)));
            }
        }

        public void Finish()
        {
            TraderSubscriber.StopResponses();
            TraderSubscriber.Close();
        }
    }
}
