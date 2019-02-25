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
    public class TestsLibrary : TestEngine
    {
        public void BuySource()
        {
            Clear();
            TradeSettings.Settings.BuyerCommissionAccount = CommissionAccount.SOURCE_ACCOUNT;
            TradeSettings.Settings.BuyerTakerCommissionProgressive = 7;
            TradeSettings.Settings.BuyerCommissionMethod = CommissionMethod.EXACT_VALUE;
            ConfiguratorService.SaveTradeSetting(User.UserId, TradeSettings.Settings);

            long sentOrderTime = StompWebSocketService.ConvertToUnixTimestamp(DateTime.Now);
            Order = new OrderCrypto() { Destination = Constants.Exchange, Quantity = 0.01, Side = Side.BUY, Type = OrderType.MARKET, SecurityId = Symbol };
            TraderSubscriber.SendOrder(Order, OrderRequest);
            Thread.Sleep(2000);
            ManagerSubscriber.CheckTransactions(GetTransactions, sentOrderTime);
            Thread.Sleep(2000);
            TraderSubscriber.CheckBalance(CheckNewBalance);
            CheckCommissions();
        }
    }
}
