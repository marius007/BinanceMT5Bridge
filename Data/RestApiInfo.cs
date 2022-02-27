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

namespace Binance2MT5.Data
{
    class MyTradesInfo
    {
        public string symbol;
        public DateTime lastRqMyTrades;
        public WebCallResult<IEnumerable<BinanceTrade>> responseMyTrades;
    }
}
