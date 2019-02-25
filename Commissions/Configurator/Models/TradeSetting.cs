using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Commissions.Configurator.Models
{
    public enum CommissionMethod
    {
        EXACT_VALUE = 1,
        TERM_TICKS,
        QUANTITY_PERCENT
    }
    public enum CommissionAccount
    {
        SOURCE_ACCOUNT = 1,
        DESTINATION_ACCOUNT
    }

    public enum RolloverCommissionMethod
    {
        COMMISSION_IN_QUANTITY_PERCENT = 1,
        COMMISSION_IN_TERM_TICKS
    }

    public class TradeSetting
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("tradingSettingsId")]
        public string TradingSettingsId { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("isSuspendedForTrading")]
        public bool? IsSuspendedForTrading { get; set; }

        [JsonProperty("normalizationAlgo")]
        public string NormalizationAlgo { get; set; }

        [JsonProperty("bookSourceList")]
        public string BookSourceList { get; set; }

        [JsonProperty("tradeSourceList")]
        public string TradeSourceList { get; set; }

        [JsonProperty("spreadOrMarkup")]
        public double? SpreadOrMarkup { get; set; }

        [JsonProperty("priceRounding")]
        public double? PriceRounding { get; set; }

        [JsonProperty("minimumQuantity")]
        public double? MinimumQuantity { get; set; }

        [JsonProperty("maximumQuantity")]
        public double? MaximumQuantity { get; set; }

        [JsonProperty("defaultQuantity")]
        public double? DefaultQuantity { get; set; }

        [JsonProperty("quantityIncrement")]
        public double? QuantityIncrement { get; set; }

        [JsonProperty("targetPriceMinPercent")]
        public double? TargetPriceMinPercent { get; set; }

        [JsonProperty("targetPriceMaxPercent")]
        public double? TargetPriceMaxPercent { get; set; }

        [JsonProperty("orderMaxTTL")]
        public string OrderMaxTtl { get; set; }

        [JsonProperty("buyerTakerCommissionFlat")]
        public double? BuyerTakerCommissionFlat { get; set; }

        [JsonProperty("buyerTakerCommissionProgressive")]
        public double? BuyerTakerCommissionProgressive { get; set; }

        [JsonProperty("sellerTakerCommissionFlat")]
        public double? SellerTakerCommissionFlat { get; set; }

        [JsonProperty("sellerTakerCommissionProgressive")]
        public double? SellerTakerCommissionProgressive { get; set; }

        [JsonProperty("buyerMakerCommissionFlat")]
        public double? BuyerMakerCommissionFlat { get; set; }

        [JsonProperty("buyerMakerCommissionProgressive")]
        public double? BuyerMakerCommissionProgressive { get; set; }

        [JsonProperty("buyerCommissionAccount")]
        public CommissionAccount? BuyerCommissionAccount { get; set; }

        [JsonProperty("buyerCommissionMethod")]
        public CommissionMethod? BuyerCommissionMethod { get; set; }

        [JsonProperty("sellerMakerCommissionFlat")]
        public double? SellerMakerCommissionFlat { get; set; }

        [JsonProperty("sellerMakerCommissionProgressive")]
        public double? SellerMakerCommissionProgressive { get; set; }

        [JsonProperty("sellerCommissionMethod")]
        public CommissionMethod? SellerCommissionMethod { get; set; }

        [JsonProperty("sellerCommissionAccount")]
        public CommissionAccount? SellerCommissionAccount { get; set; }

        [JsonProperty("takerReserveMultiplier")]
        public double? TakerReserveMultiplier { get; set; }

        [JsonProperty("rolloverCommissionDays")]
        public string RolloverCommissionDays { get; set; }

        [JsonProperty("rolloverCommissionMethod")]
        public RolloverCommissionMethod? RolloverCommissionMethod { get; set; }

        [JsonProperty("rolloverCommissionTime")]
        public string RolloverCommissionTime { get; set; }

        [JsonProperty("rolloverCommissionHighRatePercent")]
        public double? RolloverCommissionHighRatePercent { get; set; }

        [JsonProperty("rolloverCommissionLowRatePercent")]
        public double? RolloverCommissionLowRatePercent { get; set; }

        [JsonProperty("rolloverCommissionHighVolumeDefault")]
        public double? RolloverCommissionHighVolumeDefault { get; set; }

        [JsonProperty("availableDestinationList")]
        public string AvailableDestinationList { get; set; }

        [JsonProperty("defaultDestination")]
        public string DefaultDestination { get; set; }

        [JsonProperty("isAvailable")]
        public bool? IsAvailable { get; set; }

        [JsonProperty("isAlive")]
        public bool? IsAlive { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("creationTime")]
        public string CreationTime { get; set; }

        [JsonProperty("modificationTime")]
        public string ModificationTime { get; set; }
        //"0001-01-01T00:00:00"
        public static string GetStringTime(DateTime time)
        {
            return GetYear(time.Year) + "-" + GetTwoSymbolsForNumber(time.Month) + "-" +
                   GetTwoSymbolsForNumber(time.Day) + "T" + GetTwoSymbolsForNumber(time.Hour) + ":" +
                   GetTwoSymbolsForNumber(time.Minute) + ":" + GetTwoSymbolsForNumber(time.Second) + "." +
                   GetTwoSymbolsForNumber(time.Millisecond) + "Z";
        }

        private static string GetYear(int number)
        {
            if (number < 10)
                return "000" + number;
            if (number < 100)
                return "00" + number;
            if (number < 1000)
                return "0" + number;
            return number.ToString();
        }

        private static string GetTwoSymbolsForNumber(int number)
        {
            if (number < 10)
                return "0" + number;
            return number.ToString();
        }
    }

    public class FullTradeSettings
    {
        [JsonProperty("settings")]
        public TradeSetting Settings { get; set; }

        [JsonProperty("parentSettings")]
        public TradeSetting ParentSettings { get; set; }
    }
}
