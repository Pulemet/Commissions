using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commissions.Configurator;
using Commissions.Configurator.Models;
using Commissions.CryptoCortex;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Models.Orders;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscription;
using Commissions.Subscriptions;
using Commissions.Tests;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;

namespace Commissions
{

    public class Program
    {
        static void Main(string[] args)
        {
            TestsLibrary commissions = new TestsLibrary();
            commissions.Initialize("BTC", "USD");
            Thread.Sleep(1000);

            commissions.TestsConstructor("MarketBuySourcePercent", OrderType.MARKET, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketSellSourcePercent", OrderType.MARKET, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketBuyDestinationPercent", OrderType.MARKET, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketSellDestinationPercent", OrderType.MARKET, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            Thread.Sleep(1000);

            commissions.UpdateCurrency("BCH", "USD");
            commissions.Exchange = "SGEMINI";
            Thread.Sleep(2000);
            commissions.TestsConstructor("MarketBuySourceTick", OrderType.MARKET, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketSellSourceTick", OrderType.MARKET, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketBuyDestinationTick", OrderType.MARKET, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketSellDestinationTick", OrderType.MARKET, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            Thread.Sleep(1000);

            commissions.InitializeTrader();
            commissions.UpdateCurrency("BTC", "USD");
            commissions.Exchange = "DELTIX";
            Thread.Sleep(2000);
            commissions.TestsConstructor("LimitBuySourcePercentPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, true);
            commissions.TestsConstructor("LimitSellDestinationTickPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, true);
            Thread.Sleep(1000);

            Console.WriteLine("{0}/{1} Tests Passed.", commissions.PassedTests.Count, commissions.PassedTests.Count + commissions.FailedTests.Count);
            Console.ReadLine();
            commissions.CloseConnections();
        }

    }
}
