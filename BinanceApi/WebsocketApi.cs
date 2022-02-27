using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using Binance2MT5.Data;
using Binance.Net.Objects.Spot;
using Binance.Net.Enums;

namespace Binance2MT5.BinanceApi
{
    public class WebsocketApi
    {
        private BinanceSocketClient socketClient;
        public WebsocketApi()
        {
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"),
                LogVerbosity = LogVerbosity.Error,
                LogWriters = new List<TextWriter> { Console.Out }
            });

            socketClient = new BinanceSocketClient();
        }

        public void WebsocketTickSubscribe(string symbol)
        {
            var successTrades = socketClient.FuturesUsdt.SubscribeToBookTickerUpdates(symbol, (data) =>
            {
                TickElement tick = new TickElement();
                tick.symbol = data.Symbol;
                tick.time = (int)UnixTime.ToUnixTime(DateTime.UtcNow);
                tick.bid = (double)data.BestBidPrice;
                tick.ask = (double)data.BestAskPrice;
                tick.volume = (long)(data.BestBidQuantity + data.BestAskQuantity);
                TickInfo.SetTick(tick);
            });
            var successKline = socketClient.FuturesUsdt.SubscribeToKlineUpdates(symbol, KlineInterval.OneMinute, (data) =>
            {
                Candle candle = new Candle();
                candle.symbol = data.Symbol;
                candle.startTime = (int)(data.Data.OpenTime - new DateTime(1970, 1, 1)).TotalSeconds;
                candle.open = (double)data.Data.Open;
                candle.close = (double)data.Data.Close;
                candle.high = (double)data.Data.High;
                candle.low = (double)data.Data.Low;
                candle.volume = (long)(data.Data.QuoteVolume / 1000.0m) + 1;
                candle.final = data.Data.Final;
                CandleInfo.SetCandle(candle);
            });
        }

        public async Task WebsocketUnsubscribeAll()
        {
            await socketClient.UnsubscribeAll();
        }
    }
}
