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
            DateTime startTime = DateTime.Now;
            TestsLibrary commissions = new TestsLibrary();
            commissions.Initialize("BTC", "USD");
            Thread.Sleep(1000);
            
            // Market QUANTITY_PERCENT
            commissions.TestsConstructor("MarketBuySourcePercent", OrderType.MARKET, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketSellSourcePercent", OrderType.MARKET, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketBuyDestinationPercent", OrderType.MARKET, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("MarketSellDestinationPercent", OrderType.MARKET, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            // Market EXACT_VALUE
            commissions.TestsConstructor("MarketBuySourceExact", OrderType.MARKET, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("MarketSellSourceExact", OrderType.MARKET, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("MarketBuyDestinationExact", OrderType.MARKET, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("MarketSellDestinationExact", OrderType.MARKET, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            // Limit Aggressive QUANTITY_PERCENT
            commissions.TestsConstructor("LimitBuySourcePercentAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("LimitSellSourcePercentAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("LimitBuyDestinationPercentAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            commissions.TestsConstructor("LimitSellDestinationPercentAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, false);
            // Limit Aggressive EXACT_VALUE
            commissions.TestsConstructor("LimitBuySourceExactAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("LimitSellSourceExactAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("LimitBuyDestinationExactAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            commissions.TestsConstructor("LimitSellDestinationExactAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, false);
            Thread.Sleep(1000);
            
            commissions.UpdateCurrency("BCH", "USD");
            commissions.UpdateExchange("SGEMINI");
            Thread.Sleep(2000);
            // Market TERM_TICKS
            commissions.TestsConstructor("MarketBuySourceTick", OrderType.MARKET, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketSellSourceTick", OrderType.MARKET, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketBuyDestinationTick", OrderType.MARKET, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("MarketSellDestinationTick", OrderType.MARKET, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            // Limit Aggressive TERM_TICKS
            commissions.TestsConstructor("LimitBuySourceTickAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("LimitSellSourceTickAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("LimitBuyDestinationTickAggressive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            commissions.TestsConstructor("LimitSellDestinationTickAggressive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, false);
            Thread.Sleep(1000);

            commissions.InitializeTrader();
            commissions.UpdateExchange("DELTIX");
            Thread.Sleep(2000);
            // Limit Passive QUANTITY_PERCENT
            commissions.TestsConstructor("LimitBuySourcePercentPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, true);
            commissions.TestsConstructor("LimitSellSourcePercentPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, true);
            commissions.TestsConstructor("LimitBuyDestinationPercentPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, true);
            commissions.TestsConstructor("LimitSellDestinationPercentPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.QUANTITY_PERCENT, true);
            // Limit Passive EXACT_VALUE
            commissions.TestsConstructor("LimitBuySourceExactPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, true);
            commissions.TestsConstructor("LimitSellSourceExactPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.EXACT_VALUE, true);
            commissions.TestsConstructor("LimitBuyDestinationExactPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, true);
            commissions.TestsConstructor("LimitSellDestinationExactPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.EXACT_VALUE, true);
            // Limit Passive TERM_TICKS
            commissions.TestsConstructor("LimitBuySourceTickPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, true);
            commissions.TestsConstructor("LimitSellSourceTickPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.SOURCE_ACCOUNT, CommissionMethod.TERM_TICKS, true);
            commissions.TestsConstructor("LimitBuyDestinationTickPassive", OrderType.LIMIT, Side.BUY, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, true);
            commissions.TestsConstructor("LimitSellDestinationTickPassive", OrderType.LIMIT, Side.SELL, CommissionAccount.DESTINATION_ACCOUNT, CommissionMethod.TERM_TICKS, true);
            Thread.Sleep(1000);
            
            Console.WriteLine("{0}/{1} Tests Passed.", commissions.PassedTests.Count, commissions.PassedTests.Count + commissions.FailedTests.Count);
            var duration = DateTime.Now - startTime;
            Console.WriteLine("Tests duration: {0} min {1} sec", duration.Minutes, duration.Seconds);
            if (commissions.FailedTests.Count > 0)
            {
                Console.WriteLine("FAILED TESTS:");
                foreach (var failedTest in commissions.FailedTests)
                {
                    Console.WriteLine(failedTest);
                }
            }

            Console.ReadLine();
            commissions.CloseConnections();
        }

    }
}
