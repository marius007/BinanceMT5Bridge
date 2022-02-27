using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance2MT5.BinanceApi;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Binance.Net.Objects.Futures.FuturesData;
using System.ComponentModel;

namespace Binance2MT5.Data
{
    class OrdersInfo
    {
        public string symbol;
        public DateTime lastRequest;
        public WebCallResult<IEnumerable<BinanceFuturesOrder>> order;
    }
    class AllOrdersInfo
    {
        public string symbol;
        public DateTime lastRequest;
        public WebCallResult<IEnumerable<BinanceFuturesOrder>> allOrders;
    }

    public class PositionInfo
    {
        public long Ticket { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public string OrderType { get; set; }
        public decimal OrderSize { get; set; }
        public string OrderSymbol { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal Fee { get; set; }
        public decimal Profit { get; set; }
        public string Comment { get; set; }

        public PositionInfo(long pTicket, DateTime pOpenTime, DateTime pCloseTime,
                            string pOrderType, decimal pOrderSize, string pOrderSymbol,
                            decimal pOpenPrice, decimal pClosePrice, decimal pFee, decimal pProfit, string pComment)
        {
            Ticket = pTicket;
            OpenTime = pOpenTime;
            CloseTime = pCloseTime;
            OrderType = pOrderType;
            OrderSize = pOrderSize;
            OrderSymbol = pOrderSymbol;
            OpenPrice = pOpenPrice;
            ClosePrice = pClosePrice;
            Fee = pFee;
            Profit = pProfit;
            Comment = pComment;
        }

        public string PropertyValues(bool names = false)
        {
            string ret = "";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                if(names)
                    ret +=  descriptor.Name + ",";
                else
                    ret += descriptor.GetValue(this) + ",";
            }
            return ret;
        }
    }

    public class AllPositionsInfo
    {
        public List<PositionInfo> openPositions = new List<PositionInfo>();
        public List<PositionInfo> closedPositions = new List<PositionInfo>();
        public List<PositionInfo> unclosedPositions = new List<PositionInfo>();


        private bool SameLots(decimal lots1, decimal lots2)
        {
            if (lots1 < 0.0m)
                lots1 = -lots1;
            if (lots2 < 0.0m)
                lots2 = -lots2;

            if (lots1 < (lots2 + 0.0000001m) && (lots1 > lots2 - 0.0000001m))
                return true;
            else
                return false;
        }

        public bool AddOpenPosition(long Ticket, DateTime OpenTime, string OrderType,
                                    decimal OrderSize, string OrderSymbol, decimal OpenPrice, string Comment)
        {
            var item = openPositions.FirstOrDefault(o => o.Ticket == Ticket);
            if (item != null)
                return false; // position already exists

            if (OrderType.ToLower() == "buy" || OrderType.ToLower() == "sell")
            {
                openPositions.Add(new PositionInfo(Ticket, OpenTime, DateTime.MinValue, OrderType.ToLower(), OrderSize, OrderSymbol, OpenPrice, 0.0m, 0.0m, 0.0m,Comment));
            }
            return true;
        }

        public bool ClosePosition(long pTicket,DateTime pCloseTime, string pOrderType, decimal pOrderSize, string pOrderSymbol, decimal pClosePrice,string pComment,decimal fee)
        {

            List<int> removeList = new List<int>();
            int index = 0;
            foreach (PositionInfo position in openPositions)
            {
                if ( ( pOrderType.ToLower() == "buy" || pOrderType.ToLower() == "sell")  &&
                     ( position.OrderSymbol == pOrderSymbol ) &&
                     ( position.OrderType != pOrderType.ToLower() ) &&
                     (position.Comment == pComment) &&
                     SameLots(position.OrderSize, pOrderSize) 
                   )
                {
                    position.CloseTime = pCloseTime;
                    position.ClosePrice = pClosePrice;
                    if (position.OrderType == "buy")
                        position.Profit = (position.ClosePrice - position.OpenPrice) * position.OrderSize;
                    else
                        position.Profit = (position.OpenPrice - position.ClosePrice) * position.OrderSize;
                    position.Fee = fee;
                    closedPositions.Add(position);
                    removeList.Add(index);
                    break;
                }
                index++;
            }

            if (removeList.Count == 0)
            {
                unclosedPositions.Add(new PositionInfo(pTicket,DateTime.MinValue,pCloseTime, pOrderType, pOrderSize, pOrderSymbol, 0.0m, pClosePrice,0.0m , 0.0m,"unclosed:"+pComment));
            }

            foreach (var element in removeList)
            {
                openPositions.RemoveAt(element);
            }

            return true;
        }
    }

    class DistinctOrderComparer : IEqualityComparer<BinanceFuturesOrder>
    {

        public bool Equals(BinanceFuturesOrder x, BinanceFuturesOrder y)
        {
            return x.OrderId == y.OrderId;
        }

        public int GetHashCode(BinanceFuturesOrder obj)
        {
            return obj.OrderId.GetHashCode();
        }
    }
}
