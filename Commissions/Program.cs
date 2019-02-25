using System;
using System.Collections.Generic;
using System.Linq;
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
using Commissions.Tests;
using Newtonsoft.Json;

namespace Commissions
{
    class Program
    {
        private const string ConfiguratorUrl = "http://staging-config.cryptowebui.com";
        private const string CryptoUrl = "http://staging.cryptowebui.com";
        private const string CryptoTraderSubscribe = "ws://staging.cryptowebui.com/websocket/v1?trader_0";
        private const string CryptoManagerSubscribe = "ws://staging.cryptowebui.com/websocket-manager/v1?manager_1";
        private const string ConfiguratorAuthorization = "";
        private const string CryptoAuthorization = "Basic d2ViOg==";
        private const string ConfiguratorUserName = "OperatorUsername";
        private const string ConfiguratorUserPass = "OperatorPassword";
        private const string CryptoUserName = "Ekaterina_runec@mail.ru";
        private const string CryptoUserPass = "12!@qwQW";

        static void Main(string[] args)
        {
            TermTickCommissions commissions = new TermTickCommissions();
            commissions.Initialize("BTC", "USD");
            Thread.Sleep(2000);
            commissions.BuySource();
            Console.ReadLine();
            commissions.CloseConnections();
            Console.ReadLine();
            /*
            RestService configuratorRestService = new RestService(ConfiguratorUrl, "/token", ConfiguratorAuthorization);
            ConfiguratorRestService configuratorService = new ConfiguratorRestService(configuratorRestService);
            configuratorService.Authorize(ConfiguratorUserName, ConfiguratorUserPass);

            UserForSearch user = new UserForSearch() { Name = "ekaterina_runec@mail.ru" };
            var users = configuratorService.SearchUser(user);
            Console.WriteLine(users[0].Username);
            var settings = configuratorService.GeTradeSettings(users[0].UserId);
            var stg = settings.FirstOrDefault(s => s.Settings.Symbol == "BTCUSD");
            if (stg != null)
            {
                Console.WriteLine("Symbol: {0}", stg.Settings.Symbol);
                Console.WriteLine("Buyer Taker Commission Progressive: {0}", stg.Settings.BuyerTakerCommissionProgressive);
                Console.WriteLine("Seller Taker Commission Progressive: {0}", stg.Settings.SellerTakerCommissionProgressive);
                Console.WriteLine("Buyer Commission Method: {0}", stg.Settings.BuyerCommissionMethod);
                Console.WriteLine("Seller Commission Method: {0}", stg.Settings.SellerCommissionMethod);
                //stg.Settings.ModificationTime = TradeSetting.GetStringTime(DateTime.UtcNow);
                //var newStg = configuratorService.SaveTradeSetting(users[0].UserId, stg.Settings);
            }
            try
            {
                RestService cryptoRestService = new RestService(CryptoUrl, "/oauth/token", CryptoAuthorization);
                CryptoRestService cryptoService = new CryptoRestService(cryptoRestService);
                cryptoService.Authorize(CryptoUserName, CryptoUserPass);
                TraderSubscription traderSub = new TraderSubscription(CryptoTraderSubscribe, cryptoService.GetToken);
                traderSub.ResponsesSubscribe(CheckWebSocketStatus);

                ManagerSubscription managerSub = new ManagerSubscription(CryptoManagerSubscribe, cryptoService.GetToken);
                managerSub.ResponsesSubscribe(CheckWebSocketStatus);

                Thread.Sleep(1000);
                traderSub.CheckBalance(PrintBalance);

                OrderCrypto order = new OrderCrypto() { Destination = "SBITFINEX", Quantity = 0.05, Side = Side.BUY, Type = OrderType.MARKET, SecurityId = "BTCUSD" };
                Thread.Sleep(1000);

                long sentOrderTime = StompWebSocketService.ConvertToUnixTimestamp(DateTime.Now);
                Console.WriteLine("Send {0} order on {1}", order.Side, order.Quantity);
                traderSub.SendOrder(order, PrintOrder);
                Thread.Sleep(3000);

                managerSub.CheckTransactions(PrintTransactions, sentOrderTime);
                Thread.Sleep(1000);
                traderSub.CheckBalance(PrintBalance);
                Thread.Sleep(1000);
                Console.ReadLine();
                traderSub.StopResponses(CheckWebSocketStatus);
                managerSub.StopResponses(CheckWebSocketStatus);
                traderSub.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */


        }
        public static void PrintOrder(string message)
        {
            Console.WriteLine("Enter inside in PrintOrder!\n Status: {0}", message.Substring(0, 3));
        }

        public static void CheckWebSocketStatus(string message)
        {
            Console.WriteLine("General subscribe! Body: {0}", message);
        }

        public static void PrintBalance(string message)
        {
            Console.WriteLine("Enter inside in PrintBalance!");
            List<BalanceDto> balances = JsonConvert.DeserializeObject<BalanceDto[]>(message.Substring(3)).ToList();
            var balanceBTC = balances.ToList().FirstOrDefault(b => b.CurrencyId == "BTC");
            if (balanceBTC != null)
            {
                Console.WriteLine("Currency {0}: {1}", balanceBTC.CurrencyId, balanceBTC.Balance);
            }
            var balanceUSD = balances.ToList().FirstOrDefault(b => b.CurrencyId == "USD");
            if (balanceUSD != null)
            {
                Console.WriteLine("Currency {0}: {1}", balanceUSD.CurrencyId, balanceUSD.Balance);
            }
        }

        public static void PrintTransactions(string message)
        {
            Console.WriteLine("Enter inside in PrintBalance!");
            List<TransactionDto> transactions = JsonConvert.DeserializeObject<TransactionDto[]>(message.Substring(3)).ToList();
            foreach (var transact in transactions)
            {
                Console.WriteLine("Currency {0}: Type: {1} Quantity: {2}", transact.CurrencyId, transact.Type, transact.Amount);
            }
        }


    }
}
