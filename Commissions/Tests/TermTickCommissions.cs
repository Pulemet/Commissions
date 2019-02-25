using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commissions.Configurator;
using Commissions.Configurator.Models;
using Commissions.CryptoCortex;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscription;
using Commissions.Subscriptions;
using Newtonsoft.Json;
using Type = Commissions.CryptoCortex.Models.Type;

namespace Commissions.Tests
{
    public class TermTickCommissions : BaseScenario
    {
        public TermTickCommissions()
        {

        }

        public void BuySource()
        {
            long sentOrderTime = StompWebSocketService.ConvertToUnixTimestamp(DateTime.Now);
            Order = new OrderCrypto() { Destination = Constants.Exchange, Quantity = 0.01, Side = Side.SELL, Type = OrderType.MARKET, SecurityId = Symbol };
            TraderSubscriber.SendOrder(Order, OrderRequest);
            Thread.Sleep(2000);
            ManagerSubscriber.CheckTransactions(GetTransactions, sentOrderTime);
            Thread.Sleep(2000);
            TraderSubscriber.CheckBalance(CheckNewBalance);
            CheckCommissions();
        }

        public void CheckCommissions()
        {
            if (Order.Side == Side.SELL)
            {
                double commissions =
                    BaseTransactions.FindAll(t => t.Type == Type.trading_commission).Sum(t => t.Amount);
                double pv = TradeSettings.Settings.SellerTakerCommissionProgressive != null
                        ? (double)TradeSettings.Settings.SellerTakerCommissionProgressive
                        : (double)TradeSettings.ParentSettings.SellerTakerCommissionProgressive / 100;
                double calcCommissions = BaseTransactions.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount) * pv;
                if(CompareDouble(commissions, calcCommissions))
                {
                    Console.WriteLine("Commissions: {0}, Calculate commissions: {1}", commissions, calcCommissions);
                }
                else
                {
                    Console.WriteLine("Error! Commissions: {0}, Calculate commissions: {1}", commissions, calcCommissions);
                }
            }
        }

    }
}
