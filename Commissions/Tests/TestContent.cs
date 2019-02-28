using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commissions.Configurator;
using Commissions.Configurator.Models;
using Commissions.CryptoCortex;
using Commissions.CryptoCortex.Models;
using Commissions.CryptoCortex.Models.Orders;
using Commissions.CryptoCortex.Subscriptions;
using Commissions.Rest;
using Commissions.Subscriptions;

namespace Commissions.Tests
{
    public enum OrderStatus
    {
        CANCELED,
        PENDING,
        FILLED,
        INDEFINITE
    }
    public class TestContent
    {
        public OrderCrypto Order { get; set; }
        public double InitBaseBalance { get; set; }
        public double InitTermBalance { get; set; }
        public FullTradeSettings TradeSettings { get; set; }
        public List<TransactionDto> BaseTransactions { get; set; }
        public List<TransactionDto> TermTransactions { get; set; }
        public bool IsOrderSent { get; set; }
        public bool IsSuccess { get; set; }
        public string SentOrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public TestContent()
        {
            OrderStatus = OrderStatus.INDEFINITE;
            IsSuccess = true;
            IsOrderSent = false;
        }
    }
}
