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
using Commissions.CryptoCortex.Models.Orders;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscription;
using Commissions.Subscriptions;
using Newtonsoft.Json;
using System.Reflection;
using Type = Commissions.CryptoCortex.Models.Type;

namespace Commissions.Tests
{
    /*
    public class TestCommissionAttribute : System.Attribute
    {
        public TestCommissionAttribute()
        { }
    }
    */

    public class TestsLibrary : TestEngine
    {
        public List<string> FailedTests = new List<string>();
        public List<string> PassedTests = new List<string>();

        /*
        public void Invoke(string testName)
        {
            // attribute type we search
            System.Type attributeType = typeof(TestCommissionAttribute);

            // find method
            var methodInfo = this.GetType().GetMethods().FirstOrDefault(m =>
                m.GetCustomAttributes(attributeType).Cast<TestCommissionAttribute>().Any() &&
                string.Equals(m.Name, testName, StringComparison.InvariantCultureIgnoreCase));

            if (methodInfo != null)
            {
                // method found
                PrintTestName(methodInfo.Name);
                InitializeTestContent();
                try
                {
                    methodInfo.Invoke(this, new object[] { methodInfo.Name });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void InvokeAll()
        {
            System.Type attributeType = typeof(TestCommissionAttribute);

            var methodsInfo = this.GetType().GetMethods().Where(m =>
                m.GetCustomAttributes(attributeType).Cast<TestCommissionAttribute>().Any()).ToList();

            foreach (var method in methodsInfo)
            {
                PrintTestName(method.Name);
                InitializeTestContent();
                try
                {
                    method.Invoke(this, new object[] { method.Name });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void PrintTestName(string testName)
        {
            Console.WriteLine("Start test: {0}", testName);
        }
        */

        public void TestsConstructor(string testName, OrderType orderType, Side orderSide, CommissionAccount account, CommissionMethod method, bool isPassive)
        {
            InitializeTestContent();

            TestContent.TradeSettings.Settings.BuyerTakerCommissionProgressive = 7;
            TestContent.TradeSettings.Settings.SellerTakerCommissionProgressive = 5;
            TestContent.TradeSettings.Settings.BuyerMakerCommissionProgressive = 6;
            TestContent.TradeSettings.Settings.SellerMakerCommissionProgressive = 4;

            if (orderSide == Side.BUY)
            {
                TestContent.TradeSettings.Settings.BuyerCommissionAccount = account;
                TestContent.TradeSettings.Settings.BuyerCommissionMethod = method;
            }
            if (orderSide == Side.SELL)
            {
                TestContent.TradeSettings.Settings.SellerCommissionAccount = account;
                TestContent.TradeSettings.Settings.SellerCommissionMethod = method;
            }

            ConfiguratorService.SaveTradeSetting(User.UserId, TestContent.TradeSettings.Settings);

            Order = new OrderCrypto()
            {
                Destination = Exchange,
                Quantity = method == CommissionMethod.TERM_TICKS ? 2 : 0.01,
                Side = orderSide,
                Type = orderType,
                SecurityId = Symbol,
            };

            if (orderType == OrderType.LIMIT)
            {
                Order.Price = CalcAggressiveOrderPrice(orderSide);
                Order.TimeInForce = isPassive ? TimeInForce.DAY : TimeInForce.IOC;
            }

            GeneralCheck(testName, isPassive);
        }

        private double CalcAggressiveOrderPrice(Side side)
        {
            return side == Side.BUY ? BestAsk * 1.01 : BestBid * 0.99;
        }

        private double CalcPassiveOrderPrice(Side side)
        {
            double coeff = 0.001;

            if (side == Side.BUY)
            {
                return CompareDouble(BestAsk, 0) ? 2050 : BestAsk - coeff;
            }
            return CompareDouble(BestBid, 0) ? 1950 : BestBid + coeff;
        }

        private void GeneralCheck(string testName, bool isPassive)
        {
            long sentOrderTime = StompWebSocketService.ConvertToUnixTimestamp(DateTime.Now.AddSeconds(-1));
            Logger.Debug("Send order time: {0}", TradeSetting.GetStringTime(DateTime.Now));

            if(Order.Type == OrderType.LIMIT && isPassive)
                WaitSendingLimitOrder();
            else
            {
                TraderSubscriber.SendOrder(Order, OrderResponce);
                TestContent.OrderStatus = OrderStatus.PENDING;
            }

            WaitOrderStatus();

            ManagerSubscriber.GetTransactions(SaveTransactions, sentOrderTime);
            Thread.Sleep(1000);
            if (TestContent.IsOrderSent)
            {
                try
                {
                    CheckCommissions(isPassive);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("While calculating commissions an error occured: {0}", ex.Message);
                    Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                }

                try
                {
                    TraderSubscriber.CheckBalance(CheckNewBalance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("While checking balance an error occured: {0}", ex.Message);
                    Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                }
                Thread.Sleep(2000);
            }
            AddTestResult(TestContent.IsSuccess && TestContent.IsOrderSent, testName);
        }

        private void AddTestResult(bool isSuccess, string testName)
        {
            if (isSuccess)
            {
                PassedTests.Add(testName);
                Console.WriteLine("Test {0} is success", testName);
            }
            else
            {
                FailedTests.Add(testName);
                Console.WriteLine("Test {0} is fail", testName);
            }
        }
        
        private void WaitSendingLimitOrder()
        {
            int counter = 0;
            Order.Price = CalcPassiveOrderPrice(Order.Side);
            TestContent.OrderStatus = OrderStatus.PENDING;
            TraderSubscriber.SendOrder(Order, OrderResponce);

            while (!TestContent.IsOrderSent && counter++ < 100)
            {
                Thread.Sleep(100);
            }

            if (TestContent.OrderStatus == OrderStatus.PENDING)
            {
                try
                {
                    TestTrader.ExecuteOrder(Order);
                }
                catch (Exception ex)
                {
                    TestContent.OrderStatus = OrderStatus.INDEFINITE;
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void WaitOrderStatus()
        {
            int counter = 0;
            while (TestContent.OrderStatus == OrderStatus.PENDING && counter++ < 100)
            {
                Thread.Sleep(100);
            }
            Thread.Sleep(1000);
        }

        /*
        public void WaitFillingLimitOrder()
        {
            while (TestContent.OrderStatus != OrderStatus.FILLED)
            {
                TestContent.OrderStatus = OrderStatus.PENDING;
                Order.Price = CalcPassiveOrderPrice(Order.Side);
                TraderSubscriber.SendOrder(Order, OrderResponce);
                WaitLimitOrderStatus();
                if (TestContent.OrderStatus == OrderStatus.INDEFINITE)
                {
                    Console.WriteLine("Impossible to execute LIMIT order");
                    TestContent.IsSuccess = false;
                    return;
                }
            }
        }

        public void WaitLimitOrderStatus()
        {
            int counter = 0;
            while (TestContent.OrderStatus == OrderStatus.PENDING)
            {
                if(counter++ == 70)
                    CancelOrder();
                Thread.Sleep(100);
            }
        }
        */
    }
}
