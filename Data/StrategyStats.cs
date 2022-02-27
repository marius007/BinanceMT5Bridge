using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance2MT5.Data
{
    class StrategyStats
    {
        public string StrategyName = "";
        public decimal Profit = 0.0m;
        public long TotalTrades = 0;
        public decimal TradedVolume = 0.0m;
        public decimal Fees = 0.0m;
        public decimal GrossProfit = 0.0m;
        public decimal GrossLoss = 0.0m;
        // Profit Factor calculated from GrossProfit/GrossLoss
        public long WiningTrades = 0;
        public long LoosingTades = 0;

        public StrategyStats(string pStrategyName)
        {
            StrategyName = pStrategyName;
        }

        public void AddTradeInfo(PositionInfo trade)
        {
            Profit += trade.Profit;
            TotalTrades += 1;
            Fees = Fees + trade.Fee;

            if (trade.OrderSize < 0.0m)
                TradedVolume = TradedVolume - trade.OrderSize;
            else
                TradedVolume = TradedVolume + trade.OrderSize;

            if (trade.Profit > 0.0m)
            {
                GrossProfit += trade.Profit;
                WiningTrades += 1;
            }
            else
            {
                GrossLoss += trade.Profit;
                LoosingTades += 1;
            }
        }
    }

    class AllStrategyStats
    {
        public List<StrategyStats> allStrategiesStats = new List<StrategyStats>();

        public AllStrategyStats(AllPositionsInfo positions)
        {
            foreach (PositionInfo position in positions.closedPositions)
            {
                StrategyStats strategy = allStrategiesStats.FirstOrDefault(o => o.StrategyName == position.Comment);
                if (strategy == null)
                {
                    strategy = new StrategyStats(position.Comment);
                    allStrategiesStats.Add(strategy);
                }

                strategy.AddTradeInfo(position);
            }
        }

    }
}
