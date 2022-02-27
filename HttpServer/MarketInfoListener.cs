using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Binance2MT5.BinanceApi;
using Binance2MT5.Data;

namespace Binance2MT5.HttpServer
{
    public class MarketInfoListener : ListenerThread
    {
        private WebsocketApi ws;
        public MarketInfoListener(string uri) : base(uri)
        {
            // Binance Api
            ws = new WebsocketApi();
        }

        public override string GetResponse(HttpListenerRequest request)
        {
            string result = "";

            switch (request.Url.LocalPath)
            {
                // http://127.0.0.1:1314/MarketInfo/subscribe?symbols=BTCUSDT,ETHUSDT,BCHUSDT&bars=1000
                case "/MarketInfo/subscribe":
                    try
                    {
                        int bars = 500;
                        string[] symbols = request.QueryString["symbols"].Split(',');
                        bars = Convert.ToInt32(request.QueryString["bars"]);
                       
                        foreach (string symbol in symbols)
                        {
                            ws.WebsocketTickSubscribe(symbol);
                            DateTime startTime = DateTime.Now.AddSeconds(-60 * bars);
                            HistoryDownload.AddDownload(symbol, startTime);
                          }

                        if (symbols.Count() > 0)
                        {
                            result = "Suscribed to channes.";
                        }
                        else
                        {
                            result = "Invalid parameters.";
                        }
                    }
                    catch (Exception e)
                    {
                        result = e.Message;
                        break;
                    }
                    break;

                // http://127.0.0.1:1314/MarketInfo/getinfo
                case "/MarketInfo/getinfo":
                    result = "";

                    // Last tick
                    var ticks = TickInfo.GetTicks();
                    foreach (TickElement tick in ticks)
                    {
                        result += string.Format("t,{0},{1},{2},{3},{4}\n",
                                                tick.symbol, tick.time, tick.bid, tick.ask, tick.volume);
                    }
                    
                    // Last two candles
                    var candles = CandleInfo.GetCandles();
                    foreach (CandleElement candle in candles)
                    {
                        /* Commented because MT5 fails if last candle updated multiple times
                        result += string.Format("c,{0},{1},{2},{3},{4},{5},{6}\n",
                                                candle.last.symbol, candle.last.startTime, candle.last.open,
                                                candle.last.high, candle.last.low, candle.last.close, candle.last.volume);
                        */
                        if (candle.sendTime > 0 &&
                             candle.closed.startTime >= (candle.sendTime + 120) )
                        {
                            DateTime startDownload = UnixTime.FromUnixTime(candle.sendTime);
                            Console.WriteLine("Connection lost. Redownload candles from:{0}", startDownload);
                            HistoryDownload.AddDownload(candle.closed.symbol, startDownload);
                        }

                        if (candle.closed.startTime > candle.sendTime)
                        {
                            result += string.Format("c,{0},{1},{2},{3},{4},{5},{6}\n",
                                                    candle.closed.symbol, candle.closed.startTime, candle.closed.open,
                                                    candle.closed.high, candle.closed.low, candle.closed.close, candle.closed.volume);
                            CandleInfo.SetCandleSendTime(candle, candle.closed.startTime);
                        }
                    }

                    // Downloaded candles
                    for (int i = 0; i < 100; i++)
                    {
                        if (HistoryDownload.GetHistorySize() > 0)
                        {
                            var histCandle = HistoryDownload.GetCandle();
                            result += string.Format("c,{0},{1},{2},{3},{4},{5},{6}\n",
                                                    histCandle.symbol, histCandle.startTime, histCandle.open,
                                                    histCandle.high, histCandle.low, histCandle.close, histCandle.volume);
                            HistoryDownload.RemoveCandle(histCandle);
                        }
                        else
                        {
                            break;
                        }
                    }

                    break;

                // http://127.0.0.1:1314/MarketInfo/ping
                case "/MarketInfo/ping":
                    result = "OK";
                    break;

                // http://127.0.0.1:1314/MarketInfo/adddownload?symbol=BTCUSD&starttime=12345553
                case "/MarketInfo/adddownload":
                    string downloadsymbol = request.QueryString["symbol"];
                    long starttime = Convert.ToInt64(request.QueryString["starttime"]);
                    HistoryDownload.AddDownload(downloadsymbol, UnixTime.FromUnixTime(starttime));
                    result = "OK";
                    break;

                default:
                    result = "Wrong local path.";
                    break;
            }

            return result;
        }
    }
}
