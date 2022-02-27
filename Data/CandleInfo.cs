using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance2MT5.Data
{
    public class Candle
    {
        public string symbol = "";
        public long startTime = 0;
        public double open = 0.0;
        public double close = 0.0;
        public double high = 0.0;
        public double low = 0.0;
        public long volume = 0;
        public bool final = false;

        public void Copy(Candle source)
        {
            symbol = source.symbol;
            startTime = source.startTime;
            open = source.open;
            close = source.close;
            high = source.high;
            low = source.low;
            volume = source.volume;
            final = source.final;
        }
    }

    public class CandleElement
    {
        public Candle last = new Candle();
        public Candle closed = new Candle();
        public long sendTime = 0;
        public readonly object candleLock = new object();
    }

    class CandleInfo
    {
        private static List<CandleElement> candles = new List<CandleElement>();

        public static List<CandleElement> GetCandles()
        {
            List<CandleElement> resultList = new List<CandleElement>();

            foreach (CandleElement candle in candles)
            {
                lock (candle.candleLock)
                {
                    CandleElement resultElement = new CandleElement();
                    resultElement.closed.Copy(candle.closed);
                    resultElement.sendTime = candle.sendTime;
                    resultList.Add(resultElement);
                }
            }

            return resultList;
        }

        public static void SetCandleSendTime(CandleElement source, long sendTime)
        {
            foreach (CandleElement candle in candles)
            {
                if (candle.closed.symbol == source.closed.symbol)
                {
                    candle.sendTime = sendTime;   
                }
            }
        }

        public static void SetCandle(Candle source)
        {
            CandleElement updateCandle = null;

            foreach (CandleElement candle in candles)
            {
                if (candle.last.symbol == source.symbol)
                {
                    updateCandle = candle;
                    break;
                }
            }

            if (updateCandle == null)
            {
                updateCandle = new CandleElement();
                candles.Add(updateCandle);
            }

            lock (updateCandle.candleLock)
            {
                updateCandle.last.Copy(source);
                if (source.final)
                {
                    updateCandle.closed.Copy(source);
                }
            }
        }
    }
}
