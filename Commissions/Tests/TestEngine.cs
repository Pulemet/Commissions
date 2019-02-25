using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.Configurator;
using Commissions.Configurator.Models;
using Commissions.CryptoCortex;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscriptions;
using Newtonsoft.Json;
using Type = Commissions.CryptoCortex.Models.Type;

namespace Commissions.Tests
{
    public class TestEngine
    {
        public RestService ConfiguratorRest;
        public ConfiguratorRestService ConfiguratorService;
        public RestService CryptoRest;
        public CryptoRestService CryptoService;
        public TraderSubscription TraderSubscriber;
        public ManagerSubscription ManagerSubscriber;
        public OrderCrypto Order;
        public double InitBaseBalance;
        public double InitTermBalance;
        public string BaseSymbol;
        public string TermSymbol;
        public string Symbol;
        public readonly UserForSearch ConfiguratorUser = new UserForSearch() { Name = "ekaterina_runec@mail.ru" };
        public FullTradeSettings TradeSettings;
        public List<TransactionDto> BaseTransactions;
        public List<TransactionDto> TermTransactions;
        public bool IsOrderSent;
        public UserDto User;

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
            TradeSettings = ConfiguratorService.GeTradeSettings(User.UserId).FirstOrDefault(s => s.Settings.Symbol == Symbol);

            // Crypto
            CryptoRest = new RestService(Constants.CryptoUrl, "/oauth/token", Constants.CryptoAuthorization);
            CryptoService = new CryptoRestService(CryptoRest);
            CryptoService.Authorize(Constants.CryptoUserName, Constants.CryptoUserPass);
            // Trader subscription
            TraderSubscriber = new TraderSubscription(Constants.CryptoTraderSubscribe, CryptoService.GetToken);
            TraderSubscriber.ResponsesSubscribe(CheckWebSocketStatus);
            // Manager subscription
            ManagerSubscriber = new ManagerSubscription(Constants.CryptoManagerSubscribe, CryptoService.GetToken);
            ManagerSubscriber.ResponsesSubscribe(CheckWebSocketStatus);
            // Init balance
            TraderSubscriber.CheckBalance(InitializeBalance);
        }

        private void CheckWebSocketStatus(string message)
        {
            if (message.Length > 3 && message.Substring(0, 3) != "200")
            {
                Console.WriteLine("Error! Status: {0}", message.Substring(0, 3));
            }
        }

        private void InitializeBalance(string message)
        {
            List<BalanceDto> balances = JsonConvert.DeserializeObject<BalanceDto[]>(message.Substring(3)).ToList();
            InitBaseBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == BaseSymbol).Balance;
            InitTermBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == TermSymbol).Balance;
        }

        protected void CheckNewBalance(string message)
        {
            List<BalanceDto> balances = JsonConvert.DeserializeObject<BalanceDto[]>(message.Substring(3)).ToList();
            double newBaseBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == BaseSymbol).Balance;
            double newTermBalance = (double)balances.ToList().FirstOrDefault(b => b.CurrencyId == TermSymbol).Balance;

            double baseOrderSize = BaseTransactions.Sum(t => t.Amount);
            CompareBalances(BaseSymbol, baseOrderSize, InitBaseBalance, newBaseBalance);

            double termOrderSize = TermTransactions.Sum(t => t.Amount);
            CompareBalances(TermSymbol, termOrderSize, InitTermBalance, newTermBalance);
        }

        protected void GetTransactions(string message)
        {
            List<TransactionDto> transactions = JsonConvert.DeserializeObject<TransactionDto[]>(message.Substring(3)).ToList();
            BaseTransactions = transactions.FindAll(t => t.CurrencyId == BaseSymbol);
            TermTransactions = transactions.FindAll(t => t.CurrencyId == TermSymbol);
        }

        protected void OrderRequest(string message)
        {
            IsOrderSent = IsSuccessStatus(message.Substring(0, 3), "Order is not sent, Status:");
        }

        private bool IsSuccessStatus(string status, string message)
        {
            if (status != "200")
            {
                Console.Write("{0}: {1}", message, status);
                return false;
            }

            return true;
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
            }
        }

        protected double GetCoeffMethod(CommissionMethod method, CommissionAccount account)
        {
            switch (method)
            {
                case CommissionMethod.QUANTITY_PERCENT:
                    return 0.01;
                case CommissionMethod.TERM_TICKS:
                    return 1;
            }

            return 1;
        }

        protected void CalcCommissions(List<TransactionDto> currencyForCommission, CommissionMethod method, CommissionAccount account, double pv)
        {
            string currency = currencyForCommission[0].CurrencyId;
            int precision = currency == BaseSymbol ? 4 : 3;

            double commissions =
                currencyForCommission.FindAll(t => t.Type == Type.trading_commission).Sum(t => t.Amount);

            double calcCommissions = method == CommissionMethod.EXACT_VALUE
                    ? Math.Round(BaseTransactions.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount) * pv, precision)
                    : Math.Round(currencyForCommission.FindAll(t => t.Type == Type.profit_loss).Sum(t => t.Amount) *
                                   pv * GetCoeffMethod(method, account), precision);

            calcCommissions = calcCommissions > 0 ? -calcCommissions : calcCommissions;

            if (CompareDouble(commissions, calcCommissions))
            {
                Console.WriteLine("{0} Commissions: {1}, Calculate commissions: {2}", currency, commissions, calcCommissions);
            }
            else
            {
                Console.WriteLine("Error! {0} Commissions: {1}, Calculate commissions: {2}", currency, commissions, calcCommissions);
            }
        }

        protected void CheckCommissions()
        {
            if (Order.Side == Side.BUY)
            {
                CommissionAccount account = (CommissionAccount)TradeSettings.Settings.BuyerCommissionAccount;
                CommissionMethod method = (CommissionMethod)TradeSettings.Settings.BuyerCommissionMethod;

                List<TransactionDto> currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT ?
                    TermTransactions : BaseTransactions;

                double pv = Order.Type == OrderType.MARKET
                    ? (double)TradeSettings.Settings.BuyerTakerCommissionProgressive
                    : (double)TradeSettings.Settings.BuyerMakerCommissionProgressive;

                CalcCommissions(currencyForCommission, method, account, pv);
            }

            if (Order.Side == Side.SELL)
            {
                CommissionAccount account = (CommissionAccount)TradeSettings.Settings.SellerCommissionAccount;
                CommissionMethod method = (CommissionMethod)TradeSettings.Settings.SellerCommissionMethod;

                List<TransactionDto> currencyForCommission = account == CommissionAccount.SOURCE_ACCOUNT ?
                    BaseTransactions : TermTransactions;

                double pv = Order.Type == OrderType.MARKET
                    ? (double)TradeSettings.Settings.SellerTakerCommissionProgressive
                    : (double)TradeSettings.Settings.SellerMakerCommissionProgressive;

                CalcCommissions(currencyForCommission, method, account, pv);
            }
        }

        public static bool CompareDouble(double first, double second)
        {
            return Math.Abs(first - second) < Constants.Delta;
        }

        protected void Clear()
        {
            InitBaseBalance = 0;
            InitTermBalance = 0;
            TradeSettings = null;
            BaseTransactions = new List<TransactionDto>();
            TermTransactions = new List<TransactionDto>();
            IsOrderSent = false;
            User = new UserDto();
        }

        public void CloseConnections()
        {
            TraderSubscriber.StopResponses(CheckWebSocketStatus);
            ManagerSubscriber.StopResponses(CheckWebSocketStatus);
            TraderSubscriber.Close();
            ManagerSubscriber.Close();
        }
    }
}
