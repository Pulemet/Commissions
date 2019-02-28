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
using Commissions.CryptoCortex.Models.Orders;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscriptions;
using Newtonsoft.Json;
using NLog;
using Type = Commissions.CryptoCortex.Models.Type;

namespace Commissions.Tests
{
    public class TestEngine
    {
        public readonly UserForSearch ConfiguratorUser = new UserForSearch() { Name = Constants.CryptoUserName };
        public const string OrderBookDestination = "/user/v1/market-data/";
        
        protected TestContent TestContent { get; set; }
        protected RestService ConfiguratorRest { get; set; }
        protected ConfiguratorRestService ConfiguratorService { get; set; }
        protected RestService CryptoRest { get; set; }
        protected CryptoRestService CryptoService { get; set; }
        protected TraderSubscription TraderSubscriber { get; set; }
        protected ManagerSubscription ManagerSubscriber { get; set; }
        protected OrderCrypto Order { get; set; }
        protected Security Security { get; set; }
        protected UserDto User { get; set; }
        protected string BaseSymbol { get; set; }
        protected string TermSymbol { get; set; }
        protected string Symbol { get; set; }
        protected double BestBid { get; set; }
        protected double BestAsk { get; set; }
        protected TestTrader TestTrader { get; set; }

        public string Exchange = "SBITFINEX";
        public Logger Logger = LogManager.GetCurrentClassLogger();

        public void Initialize(string baseSymbol, string termSymbol)
        {
            BaseSymbol = baseSymbol;
            TermSymbol = termSymbol;
            Symbol = baseSymbol + termSymbol;

            // Configurator
            ConfiguratorRest = new RestService(Constants.ConfiguratorUrl, "/token", Constants.ConfiguratorAuthorization);
            ConfiguratorService = new ConfiguratorRestService(ConfiguratorRest);
            ConfiguratorService.Authorize(Constants.ConfiguratorUserName, Constants.ConfiguratorUserPass);
            User = ConfiguratorService.SearchUser(ConfiguratorUser)[0];
            // Get Trading Settings
            Security = ConfiguratorService.GetSecurities().FirstOrDefault(s => s.BaseCurrency == baseSymbol && s.TermCurrency == termSymbol);
            // Crypto
            CryptoRest = new RestService(Constants.CryptoUrl, "/oauth/token", Constants.CryptoAuthorization);
            CryptoService = new CryptoRestService(CryptoRest);
            CryptoService.Authorize(Constants.CryptoUserName, Constants.CryptoUserPass);
            // Trader subscription
            TraderSubscriber = new TraderSubscription(Constants.CryptoTraderSubscribe, CryptoService.GetToken);
            Thread.Sleep(500);
            TraderSubscriber.ResponsesSubscribe();
            Thread.Sleep(500);
            // Orders
            TraderSubscriber.OrdersReceiver(CheckOrderStatus);
            Thread.Sleep(500);
            // Order book
            TraderSubscriber.OrderBookSubcribe(OrderBookDestination + Symbol, OnBestBidAsk);
            // Manager subscription
            ManagerSubscriber = new ManagerSubscription(Constants.CryptoManagerSubscribe, CryptoService.GetToken);
            ManagerSubscriber.ResponsesSubscribe();
            Thread.Sleep(500);
        }

        public void UpdateCurrency(string baseSymbol, string termSymbol)
        {
            TraderSubscriber.OrderBookUnsubscribe(OrderBookDestination + Symbol, OnBestBidAsk);
            BaseSymbol = baseSymbol;
            TermSymbol = termSymbol;
            Symbol = baseSymbol + termSymbol;
            Security = ConfiguratorService.GetSecurities().FirstOrDefault(s => s.BaseCurrency == baseSymbol && s.TermCurrency == termSymbol);

            BestBid = 0;
            BestAsk = 0;
            TraderSubscriber.OrderBookSubcribe(OrderBookDestination + Symbol, OnBestBidAsk);
        }

        public void InitializeTrader()
        {
            TestTrader = new TestTrader();
            TestTrader.Initialize();
        }

        protected void InitializeBalance(string message)
        {
            List<BalanceDto> balances = JsonConvert.DeserializeObject<BalanceDto[]>(message.Substring(3)).ToList();
            TestContent.InitBaseBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == BaseSymbol).Balance;
            TestContent.InitTermBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == TermSymbol).Balance;
        }

        protected void CheckNewBalance(string message)
        {
            List<BalanceDto> balances = JsonConvert.DeserializeObject<BalanceDto[]>(message.Substring(3)).ToList();
            double newBaseBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == BaseSymbol).Balance;
            double newTermBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == TermSymbol).Balance;

            double baseOrderSize = TestContent.BaseTransactions.Sum(t => t.Amount);
            CompareBalances(BaseSymbol, baseOrderSize, TestContent.InitBaseBalance, newBaseBalance);

            double termOrderSize = TestContent.TermTransactions.Sum(t => t.Amount);
            CompareBalances(TermSymbol, termOrderSize, TestContent.InitTermBalance, newTermBalance);
        }

        protected void SaveTransactions(string message)
        {
            List<TransactionDto> transactions = JsonConvert.DeserializeObject<TransactionDto[]>(message.Substring(3)).ToList();
            Logger.Debug("SaveTransactions time: " + TradeSetting.GetStringTime(DateTime.Now));
            Logger.Debug("Transactions count: {0}", transactions.Count);
            TestContent.BaseTransactions = transactions.FindAll(t => t.CurrencyId == BaseSymbol);
            TestContent.TermTransactions = transactions.FindAll(t => t.CurrencyId == TermSymbol);
        }

        protected void OrderResponce(string message)
        {
            Logger.Debug("OrderResponce time: " + TradeSetting.GetStringTime(DateTime.Now));
            TestContent.IsOrderSent = IsSuccessStatus(message.Substring(0, 3), "Order is not sent, Status:");
            if (TestContent.IsOrderSent)
                SaveSentOrderId(message.Substring(3));
            else if (!TestContent.IsOrderSent)
                TestContent.OrderStatus = OrderStatus.INDEFINITE;
        }

        private bool IsSuccessStatus(string status, string message)
        {
            if (status != "200")
            {
                Console.WriteLine("{0} {1}", message, status);
                return false;
            }

            return true;
        }

        private void SaveSentOrderId(string message)
        {
            TestContent.SentOrderId = JsonConvert.DeserializeObject<Order>(message).Id;
        }

        public void CancelOrder()
        {
            TraderSubscriber.CancelOrder(TestContent.SentOrderId, IsCanceled);
        }

        public void IsCanceled(string message)
        {
            if (message.Substring(0, 3) == "200")
            {
                Console.WriteLine("Order is canceled");
            }   
        }

        protected void CheckOrderStatus(string message)
        {
            Logger.Debug("CheckOrderStatus time: " + TradeSetting.GetStringTime(DateTime.Now));
            OrderResponce orderResponce = JsonConvert.DeserializeObject<OrderResponce>(message.Substring(3));
            if (orderResponce.Order.Id != TestContent.SentOrderId)
                return;
            if (orderResponce.Order.Status == Status.COMPLETELY_FILLED)
            {
                TestContent.OrderStatus = OrderStatus.FILLED;
                return;
            }
            else if (orderResponce.Order.Status == Status.CANCELED)
            {
                TestContent.OrderStatus = OrderStatus.CANCELED;
                return;
            }
            else if (orderResponce.Order.Status == Status.NEW)
            {
                TestContent.OrderStatus = OrderStatus.PENDING;
                return;
            }
            TestContent.OrderStatus = OrderStatus.INDEFINITE;
        }

        protected void OnBestBidAsk(string message)
        {
            var orderBook = JsonConvert.DeserializeObject<L2PackageDto>(message.Substring(3));
            var buyOrders = orderBook.Entries.FindAll(o => o.ExchangeId == Exchange && o.Side == Side.BUY);
            var sellOrders = orderBook.Entries.FindAll(o => o.ExchangeId == Exchange && o.Side == Side.SELL);
            if (orderBook.Type == L2PackageType.SNAPSHOT_FULL_REFRESH)
            {
                BestBid = FindBestBidAsk(buyOrders);
                BestAsk = FindBestBidAsk(sellOrders);
            }

            if (orderBook.Type == L2PackageType.INCREMENTAL_UPDATE)
            {
                UpdateBestBid(buyOrders, BestBid);
                UpdateBestAsk(sellOrders, BestAsk);
            }
            Logger.Trace("Best Bid {0}, Best Ask {1}", BestBid, BestAsk);
        }

        private double FindBestBidAsk(List<L2EntryDto> orders)
        {
            int level = orders[0].Level;
            int index = 0;

            for (int i = 1; i < orders.Count; i++)
            {
                if (orders[i].Level < level)
                {
                    index = i;
                    level = orders[i].Level;
                }
            }
            return orders[index].Price;
        }

        private double UpdateBestBid(List<L2EntryDto> orders, double bestBid)
        {
            foreach (var order in orders)
            {
                if (order.Price > bestBid)
                {
                    bestBid = order.Price;
                }
            }
            return bestBid;
        }

        private double UpdateBestAsk(List<L2EntryDto> orders, double bestAsk)
        {
            foreach (var order in orders)
            {
                if (order.Price < bestAsk)
                {
                    bestAsk = order.Price;
                }
            }
            return bestAsk;
        }

        private void CompareBalances(string symbol, double orderSize, double balance, double newBalance)
        {
            if (CompareDouble(balance, newBalance - orderSize))
            {
                Console.WriteLine("{0}: Balance is changed on size of order. Init balance = {1}, New Balance = {2}, Size of order: {3}",
                    symbol, balance, newBalance, orderSize);
            }
            else
            {
                Console.WriteLine("{0}: Error! Init balance = {1}, New Balance = {2}, Size of order: {3}",
                    symbol, balance, newBalance, orderSize);
                TestContent.IsSuccess = false;
            }
        }

        protected double GetCoeffMethod(CommissionMethod method, CommissionAccount account)
        {
            switch (method)
            {
                case CommissionMethod.QUANTITY_PERCENT:
                    return 0.01;
                case CommissionMethod.TERM_TICKS:
                    return Security.TickSize;
            }

            return 1;
        }

        protected void CalcCommissions(List<TransactionDto> currencyForCommission, CommissionMethod method, CommissionAccount account, double pv, double commissions)
        {
            string currency = currencyForCommission[0].CurrencyId;
            int precision = currency == BaseSymbol || method == CommissionMethod.TERM_TICKS ? 4 : 3;

            double calcCommissions = 0;
            if (method == CommissionMethod.EXACT_VALUE)
                calcCommissions = Math.Round(TestContent.BaseTransactions.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount) * pv, precision);
            if (method == CommissionMethod.QUANTITY_PERCENT)
                calcCommissions = Math.Round(currencyForCommission.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount) *
                                       pv * GetCoeffMethod(method, account), precision);
            if (method == CommissionMethod.TERM_TICKS)
                calcCommissions = Math.Round(Math.Pow(TestContent.BaseTransactions.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount), 2)
                           * pv * GetCoeffMethod(method, account)
                           / currencyForCommission.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount), precision);

            calcCommissions = calcCommissions > 0 ? -calcCommissions : calcCommissions;

            if (CompareDouble(commissions, calcCommissions))
            {
                Console.WriteLine("{0} Commissions: {1}, Calculate commissions: {2}", currency, commissions, calcCommissions);
            }
            else
            {
                Console.WriteLine("Error! {0} Commissions: {1}, Calculate commissions: {2}", currency, commissions, calcCommissions);
                TestContent.IsSuccess = false;
            }
        }

        protected void CheckCommissions(bool isPassive)
        {
            if (Order.Side == Side.BUY)
            {
                CommissionAccount account = (CommissionAccount)TestContent.TradeSettings.Settings.BuyerCommissionAccount;
                CommissionMethod method = (CommissionMethod)TestContent.TradeSettings.Settings.BuyerCommissionMethod;

                double pv = !isPassive
                    ? (double)TestContent.TradeSettings.Settings.BuyerTakerCommissionProgressive
                    : (double)TestContent.TradeSettings.Settings.BuyerMakerCommissionProgressive;

                List<TransactionDto> currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT
                    ? TestContent.TermTransactions : TestContent.BaseTransactions;

                double commissions = currencyForCommission.FindAll(t => t.Type == Type.trading_commission).Sum(t => t.Amount);

                if (method == CommissionMethod.TERM_TICKS)
                {
                    currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT
                        ? TestContent.BaseTransactions : TestContent.TermTransactions;
                }

                CalcCommissions(currencyForCommission, method, account, pv, commissions);
            }

            if (Order.Side == Side.SELL)
            {
                CommissionAccount account = (CommissionAccount)TestContent.TradeSettings.Settings.SellerCommissionAccount;
                CommissionMethod method = (CommissionMethod)TestContent.TradeSettings.Settings.SellerCommissionMethod;

                double pv = !isPassive
                    ? (double)TestContent.TradeSettings.Settings.SellerTakerCommissionProgressive
                    : (double)TestContent.TradeSettings.Settings.SellerMakerCommissionProgressive;

                List<TransactionDto> currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT
                    ? TestContent.BaseTransactions : TestContent.TermTransactions;

                double commissions = currencyForCommission.FindAll(t => t.Type == Type.trading_commission).Sum(t => t.Amount);

                if (method == CommissionMethod.TERM_TICKS)
                {
                    currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT
                        ? TestContent.TermTransactions : TestContent.BaseTransactions;
                }

                CalcCommissions(currencyForCommission, method, account, pv, commissions);
            }
        }

        public static bool CompareDouble(double first, double second)
        {
            return Math.Abs(first - second) < Constants.Delta;
        }

        protected void InitializeTestContent()
        {
            // Init content
            TestContent = new TestContent();
            TestContent.TradeSettings = ConfiguratorService.GeTradeSettings(User.UserId).FirstOrDefault(s => s.Settings.Symbol == Symbol);
            // Init balance
            TraderSubscriber.CheckBalance(InitializeBalance);
        }

        public void CloseConnections()
        {
            TraderSubscriber.OrderBookUnsubscribe(OrderBookDestination + Symbol, OnBestBidAsk);
            TraderSubscriber.OrdersUnsubscribe(CheckOrderStatus);
            TraderSubscriber.StopResponses();
            ManagerSubscriber.StopResponses();
            TraderSubscriber.Close();
            ManagerSubscriber.Close();
            if(TestTrader != null)
                TestTrader.Finish();
        }
    }
}
