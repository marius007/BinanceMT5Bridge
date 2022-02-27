using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Binance2MT5.BinanceApi;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Binance.Net.Objects.Futures.FuturesData;

namespace Binance2MT5.HttpServer
{
    class GetOrdersListener : ListenerThread
    {
        public GetOrdersListener(string uri) : base(uri)
        {
        }

        public override string GetResponse(HttpListenerRequest request)
        {
            string result = "";
            string symbol = "";

            switch (request.Url.LocalPath)
            {
                //http://127.0.0.1:1314/GetOrders/getaccountinfo
                case "/GetOrders/getaccountinfo":

                    if (!RestApi.IsConnected())
                    {
                        result = "Error: Keys not set";
                        break;
                    }

                    try
                    {
                        WebCallResult<BinanceFuturesAccountInfo> response = RestApi.GetFuturesAccountBalance();
                        if (response.Success)
                        {
                            foreach (var balance in response.Data.Assets)
                            {
                                if (balance.AvailableBalance > 0.00001m)
                                {
                                    result += string.Format("{0},{1},{2},{3}\n",
                                                             RestApi.GetpublicKeyCheck(), balance.Asset, balance.WalletBalance, balance.MaxWithdrawAmount);
                                }
                            }
                        }
                        else
                        {
                            result = "Error:" + response.Error.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error:" + ex.Message;
                    }
                    break;
                //http://127.0.0.1:1314/GetOrders/getpositions
                case "/GetOrders/getpositions":

                    if (!RestApi.IsConnected())
                    {
                        result = "Error: Keys not set";
                        break;
                    }

                    try
                    {
                        WebCallResult<IEnumerable<BinancePositionDetailsUsdt>> response = RestApi.GetFuturesAccountPositions();
                        if (response.Success)
                        {
                            foreach (var position in response.Data)
                            {
                                result += string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\n",
                                                            position.EntryPrice, position.MarginType, position.IsAutoAddMargin,
                                                            position.IsolatedMargin, position.Leverage, position.LiquidationPrice,
                                                            position.MarkPrice, position.MaxNotionalValue, position.PositionAmount,
                                                            position.Symbol, position.UnrealizedProfit);
                            }
                        }
                        else
                        {
                            result = "Error:" + response.Error.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error:" + ex.Message;
                    }
                    break;
                //http://127.0.0.1:1314/GetOrders/getopenorders?symbol=BTCUSDT
                case "/GetOrders/getopenorders":

                    if (!RestApi.IsConnected())
                    {
                        result = "Error: Keys not set";
                        break;
                    }

                    try
                    {
                        symbol = request.QueryString["symbol"];

                        WebCallResult<IEnumerable<BinanceFuturesOrder>> response = RestApi.GetFuturesAccountOrders(symbol);

                        if (response.Success)
                        {
                            foreach (var order in response.Data)
                            {
                                result += string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}\n",
                                                         order.Symbol, order.OrderId, order.ClientOrderId, order.Price,
                                                         order.ReduceOnly, order.OriginalQuantity, order.CumulativeQuantity, order.CumulativeQuoteQuantity,
                                                         order.Status, order.TimeInForce, order.Type, order.Side,
                                                         order.StopPrice, order.CreatedTime, order.UpdateTime, order.WorkingType);
                            }
                        }
                        else
                        {
                            result = "Error:" + response.Error.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error:" + ex.Message;
                    }
                    break;
                //http://127.0.0.1:1314/GetOrders/gethistory?symbol=BTCUSDT
                case "/GetOrders/gethistory":

                    if (!RestApi.IsConnected())
                    {
                        result = "Error: Keys not set";
                        break;
                    }

                    try
                    {
                        symbol = request.QueryString["symbol"];

                        WebCallResult<IEnumerable<BinanceFuturesOrder>> response = RestApi.GetAllOrdersSingleRequest(symbol);

                        if (response.Success)
                        {
                            foreach (var order in response.Data)
                            {
                                result += string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}\n",
                                                         order.Symbol, order.OrderId, order.ClientOrderId, order.Price,
                                                         order.ReduceOnly, order.OriginalQuantity, order.OriginalQuantity, order.CumulativeQuantity,
                                                         order.Status, order.TimeInForce, order.Type, order.Side,
                                                         order.StopPrice, order.CreatedTime, order.UpdateTime, order.WorkingType);
                            }
                        }
                        else
                        {
                            result = "Error:" + response.Error.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error:" + ex.Message;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
