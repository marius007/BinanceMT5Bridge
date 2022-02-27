using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Objects;
using Binance2MT5.Data;
using CryptoExchange.Net.Objects;
using System.ComponentModel;
using System.IO;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Enums;
using Binance.Net.Interfaces;

namespace Binance2MT5.BinanceApi
{
    class FileDownload
    {
        private static double progressPercent;

        private static DateTime GetLastTick(string fileName, string separator)
        {
            string lastLine = "";
            
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        lastLine = line;
                    }
                }
            }

            DateTime result = DateTime.MinValue;
            try
            {
                string[] split = lastLine.Split(separator[0]);
                result = Convert.ToDateTime(split[0]+" "+split[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Error return default value
            }

            return result;
        }

        public static void DownloadToFileWithRetry(bool DownloadFutures, string fileName, string symbol, DateTime startTime, DateTime endTime, string separator, BackgroundWorker bkw)
        {
            try
            {
                FileDownload.DownloadToFile(DownloadFutures,
                                            fileName,
                                            symbol,
                                            startTime,
                                            endTime,
                                            separator,
                                            bkw);
            }
            catch (Exception ex)
            {
                // Nothing to download. Binance History starts from: 08/09/2019 17:57:00
                string errorMsg = ex.Message;
                if (errorMsg.Contains("Binance History starts from"))
                {
                    try
                    {
                        string[] split = errorMsg.Split(':');
                        DateTime newStart = Convert.ToDateTime(split[1] + ":" + split[2] + ":" + split[3]);
                        // try again with new date
                        FileDownload.DownloadToFile(DownloadFutures,
                                                    fileName,
                                                    symbol,
                                                    newStart,
                                                    endTime,
                                                    separator,
                                                    bkw);
                    }
                    catch (Exception ex2)
                    {
                        throw new System.Exception(ex2.Message);
                    }
                }
                else
                {
                    throw new System.Exception(ex.Message);
                }
            }
        }

        private static void DownloadToFile(bool DownloadFutures,string folderName, string symbol, DateTime paramstartTime, DateTime endTime, string separator, BackgroundWorker bkw)
        {
            WebCallResult<IEnumerable<IBinanceKline>> result = null;
            progressPercent = 0.0;
            string fileName = Path.Combine(folderName, symbol + ".csv");

            DateTime lastDateDownloaded = DateTime.MinValue;

            if (File.Exists(fileName))
            {
                lastDateDownloaded = GetLastTick(fileName, separator);
            }

            DateTime startTime = paramstartTime; 
            if (DateTime.Compare(lastDateDownloaded, startTime) > 0)
            {
                startTime = lastDateDownloaded;
            }

            DateTime lastCandle = startTime;
            double totalCandles = (UnixTime.ToUnixTime(endTime) - UnixTime.ToUnixTime(startTime))/60 + 1;

            using (StreamWriter writeFile = File.AppendText(fileName))
            {
                do
                {
                    if (DownloadFutures)
                    {
                        result = RestApi.GetClient().FuturesUsdt.Market.GetKlines(symbol,
                                                                                   KlineInterval.OneMinute,
                                                                                   lastCandle,
                                                                                   null,
                                                                                   1000);
                    }
                    else
                    {
                        result = RestApi.GetClient().Spot.Market.GetKlines(symbol,
                                                               KlineInterval.OneMinute,
                                                               lastCandle,
                                                               null,
                                                               1000);
                    }

                    if (!result.Success)
                    {
                        throw new ArgumentException(result.Error.Message);
                    }

                    if (progressPercent < 0.000001 &&
                        result.Data.Count() > 0 &&
                       result.Data.First().OpenTime > startTime.AddDays(2))
                    {
                        throw new ArgumentException("Nothing to download. Binance History starts from: " + result.Data.First().OpenTime);
                    }

                    if (result.Data.Count() > 1)
                    {
                        foreach (var kline in result.Data)
                        {
                            string mt5Date = kline.OpenTime.ToString("yyyy.MM.dd");
                            string mt5Time = kline.OpenTime.ToString("HH:mm:00");
                            double open = (double)kline.Open;
                            double high = (double)kline.High;
                            double low = (double)kline.Low;
                            double close = (double)kline.Close;
                            int volume = (int)(kline.QuoteVolume/1000.0m) + 1;
                            int spread = 1;

                            // format line
                            string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
                                                        mt5Date, mt5Time, open, high, low,
                                                        close, volume, volume, spread);
                            if (separator != ",")
                            {
                                line = line.Replace(",", separator);
                            }
                            writeFile.WriteLine(line);
                        }
                    }

                    lastCandle = result.Data.Last().OpenTime;
                    double downloadedCandles = (UnixTime.ToUnixTime(lastCandle) - UnixTime.ToUnixTime(startTime))/60 + 1;
                    progressPercent = (downloadedCandles/totalCandles) * 100.0;
                    bkw.ReportProgress((int)progressPercent);
                }
                while (result != null &&
                        result.Data.Count() > 1 &&
                        lastCandle <= endTime);

                // flash response to file 
                writeFile.Flush();
            }
        }
    }
}
