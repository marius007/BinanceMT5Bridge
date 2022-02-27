using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Binance2MT5.Data
{
    public class TickElement
    {
        public string symbol = "";
        public int time = 0;
        public double bid = 0.0;
        public double ask = 0.0;
        public long volume = 0;
        public readonly object tickLock = new object();

        public void Copy(TickElement source)
        {
            symbol = source.symbol;
            time = source.time;
            bid = source.bid;
            ask = source.ask;
            volume = source.volume;
        }
    }

    // Tick information
    public static class TickInfo
    {
        private static List<TickElement> ticks = new List<TickElement>();
        //private static TickElement[] ticks = new TickElement[10];

        public static List<TickElement> GetTicks()
        {
            List<TickElement> resultList = new List<TickElement>();

            foreach (TickElement tick in ticks)
            {
                lock (tick.tickLock)
                {
                    TickElement resultElement = new TickElement();
                    resultElement.Copy(tick);
                    resultList.Add(resultElement);
                }
            }

            return resultList;
        }

        public static TickElement GetTick(string symbol)
        {
            TickElement resultInfo = new TickElement();

            foreach (TickElement tick in ticks)
            {
                if (tick.symbol == symbol)
                {
                    lock (tick.tickLock)
                    {
                        resultInfo.Copy(tick);
                        break;
                    }
                }
            }
            return resultInfo;
        }

        public static void SetTick(TickElement source)
        {
            TickElement updateTick = null;

            foreach (TickElement tick in ticks)
            {
                if (tick.symbol == source.symbol)
                {
                    updateTick = tick;
                    break;
                }
            }

            if (updateTick == null)
            {
                updateTick = new TickElement();
                ticks.Add(updateTick);
            }

            lock (updateTick.tickLock)
            {
                updateTick.Copy(source);
            }
        }
    }
}
