using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Binance2MT5.HttpServer;
using Binance2MT5.BinanceApi;
using Binance2MT5.Data;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using System.Globalization;
using Binance.Net.Objects.Futures.FuturesData;
using Binance.Net.Enums;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace Binance2MT5
{
    public partial class Form1 : Form
    {
        string[] binanceSymbols = { "BTCUSDT", "ETHUSDT", "BCHUSDT", "XRPUSDT", "EOSUSDT", "LTCUSDT", "TRXUSDT", "ETCUSDT", "LINKUSDT", "XLMUSDT", "ADAUSDT", "XMRUSDT", "DASHUSDT", "ZECUSDT", "XTZUSDT", "BNBUSDT", "ATOMUSDT", "ONTUSDT", "IOTAUSDT", "BATUSDT", "VETUSDT", "NEOUSDT", "QTUMUSDT", "IOSTUSDT", "THETAUSDT", "ALGOUSDT", "ZILUSDT", "KNCUSDT", "ZRXUSDT", "COMPUSDT", "OMGUSDT", "DOGEUSDT", "SXPUSDT", "KAVAUSDT", "BANDUSDT", "RLCUSDT", "WAVESUSDT", "MKRUSDT", "SNXUSDT", "DOTUSDT", "YFIUSDT", "BALUSDT", "CRVUSDT", "TRBUSDT", "YFIIUSDT", "RUNEUSDT", "SUSHIUSDT", "SRMUSDT", "BZRXUSDT", "EGLDUSDT", "SOLUSDT", "ICXUSDT", "STORJUSDT", "BLZUSDT", "UNIUSDT", "AVAXUSDT", "FTMUSDT", "HNTUSDT", "ENJUSDT", "FLMUSDT", "TOMOUSDT", "RENUSDT", "KSMUSDT", "NEARUSDT", "AAVEUSDT", "FILUSDT", "RSRUSDT", "LRCUSDT", "MATICUSDT", "OCEANUSDT", "CVCUSDT", "BELUSDT", "CTKUSDT", "AXSUSDT", "ALPHAUSDT", "ZENUSDT", "SKLUSDT", "GRTUSDT", "1INCHUSDT", "BTCBUSD", "AKROUSDT", "CHZUSDT", "SANDUSDT", "ANKRUSDT", "LUNAUSDT", "BTSUSDT", "LITUSDT", "UNFIUSDT", "DODOUSDT", "REEFUSDT", "RVNUSDT", "SFPUSDT", "XEMUSDT", "BTCSTUSDT", "COTIUSDT", "CHRUSDT", "MANAUSDT", "ALICEUSDT", "HBARUSDT", "ONEUSDT", "LINAUSDT", "STMXUSDT", "DENTUSDT", "CELRUSDT", "HOTUSDT", "MTLUSDT", "OGNUSDT", "BTTUSDT", "NKNUSDT", "SCUSDT", "DGBUSDT", "SHIBUSDT", "ICPUSDT", "BAKEUSDT", "GTCUSDT", "ETHBUSD", "KEEPUSDT", "TLMUSDT", "BNBBUSD", "ADABUSD", "XRPBUSD", "IOTXUSDT", "DOGEBUSD", "AUDIOUSDT", "RAYUSDT", "C98USDT", "MASKUSDT", "ATAUSDT", "SOLBUSD", "FTTBUSD", "DYDXUSDT", "XECUSDT", "GALAUSDT", "CELOUSDT", "ARUSDT", "KLAYUSDT", "ARPAUSDT", "NUUSDT" };
        private const string serverVersion = "2.2";
        private string marketInfoURI = "http://127.0.0.1:1314/MarketInfo/";
        private string sendOrdersURI = "http://127.0.0.1:1314/SendOrders/";
        private string getOrdersURI = "http://127.0.0.1:1314/GetOrders/";
        private const string logFileName = @".\log.txt";
        private string separator = ";";
        private string comboSymbol = "BTCUSDT";

        private MarketInfoListener marketListener;
        private SendOrdersListener sendOrdersListener;
        private GetOrdersListener getOrdersListener;

        public Form1()
        {
            InitializeComponent();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void MarginOrder_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Default combobox value
            comboBox1.SelectedItem = ",(comma)";
            // add items to combobox
            foreach (string binanceSymbol in binanceSymbols)
            {
                comboBox2.Items.Add(binanceSymbol);
            }
            comboBox2.Items.Add("ALL");
            // set default element
            comboBox2.SelectedItem = "BTCUSDT";
            dateTimePicker1.Value = DateTime.Now.AddDays(-365 * 5);

            // Redirect console to file
            StreamWriter sw = new StreamWriter(logFileName);
            sw.AutoFlush = true;
            Console.SetOut(sw);

            // read ports
            try
            {
                using (StreamReader readtext = new StreamReader(".\\ports.txt"))
                {
                    marketInfoURI = readtext.ReadLine();
                    sendOrdersURI = readtext.ReadLine();
                    getOrdersURI = readtext.ReadLine();
                }
            }
            catch (Exception)
            {
            }

            // load key
            string APIkey = "";
            string SecretKey = "";
            try
            {
                using (StreamReader readtext = new StreamReader(".\\System.AppCtx.dll"))
                {
                    APIkey = readtext.ReadLine();
                    string encrypted = readtext.ReadLine();
                    if (encrypted == "en")
                    {
                        string line = readtext.ReadLine();
                        SecretKey = StringCipher.Decrypt(line, "bnc123");
                    }
                    else
                    {
                        SecretKey = readtext.ReadLine();
                    }
                }
            }
            catch (Exception)
            {
            }

            // connect RestApi
            if (APIkey != "" && SecretKey != "")
            {
                this.textBox9.Text = APIkey;
                this.textBox12.Text = SecretKey;
                RestApi.RestApiConnect(APIkey, SecretKey);
                UpdatePositionMode();
            }

            // start download thread
            HistoryDownload.StartDownloadThread();

            // Start Listeners
            Console.WriteLine("Start server version:" + serverVersion);
            marketListener = new MarketInfoListener(marketInfoURI);
            marketListener.Start();
            sendOrdersListener = new SendOrdersListener(sendOrdersURI);
            sendOrdersListener.Start();
            getOrdersListener = new GetOrdersListener(getOrdersURI);
            getOrdersListener.Start();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            marketListener.StopListener();
            sendOrdersListener.StopListener();
            getOrdersListener.StopListener();
            HistoryDownload.StopDownload();

            RestApi.RestApiDispose();

            Environment.Exit(Environment.ExitCode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
            using (FileStream stream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        this.richTextBox1.Text += reader.ReadLine() + "\n";
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.richTextBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {
                dataGridView1.Rows.Clear();

                WebCallResult<BinanceFuturesAccountInfo> response = RestApi.GetFuturesAccountBalance();

                if (response.Success)
                {
                    foreach (var balance in response.Data.Assets)
                    {
                        if (balance.AvailableBalance > 0.00001m)
                        {
                            this.dataGridView1.Rows.Add("USDT", balance.Asset, balance.WalletBalance, balance.MaxWithdrawAmount);
                        }
                    }

                    if (this.dataGridView1.Rows.Count == 0)
                    {
                        MessageBox.Show("You have no assets to display.", "Info", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show(response.Error.Message, "Error.", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            string symbol = this.textBox1.Text;

            if (symbol == null ||
               symbol == "")
            {
                MessageBox.Show("Invalid symbol.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {
                dataGridView2.Rows.Clear();

                WebCallResult<IEnumerable<BinanceFuturesOrder>> response = RestApi.GetAllOrdersSingleRequest(symbol, dateTimePicker5.Value.Date);

                if (response.Success)
                {
                    foreach (var order in response.Data)
                    {
                        string orderTime = order.CreatedTime.ToString();
                        string updateTime = order.UpdateTime.ToString();

                        this.dataGridView2.Rows.Add(order.Symbol, order.OrderId, order.ClientOrderId, order.AvgPrice,
                                                    order.ReduceOnly, order.OriginalQuantity, order.ExecutedQuantity, order.CumulativeQuantity,
                                                    order.Status, order.TimeInForce, order.Type, order.Side, order.PositionSide,
                                                    order.StopPrice, orderTime, updateTime, order.WorkingType);
                    }

                    this.dataGridView2.Sort(this.dataGridView2.Columns[1], ListSortDirection.Descending);

                    if (this.dataGridView2.Rows.Count == 0)
                    {
                        MessageBox.Show("You have no orders to display.", "Info", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show(response.Error.Message, "Error.", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            string symbol = this.textBox6.Text;

            if (symbol == null ||
               symbol == "")
            {
                MessageBox.Show("Invalid symbol.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {
                dataGridView3.Rows.Clear();

                WebCallResult<IEnumerable<BinanceFuturesOrder>> response = RestApi.GetFuturesAccountOrders(symbol);

                if (response.Success)
                {
                    foreach (var order in response.Data)
                    {
                        string orderTime = order.CreatedTime.ToString();
                        string updateTime = order.UpdateTime.ToString();

                        this.dataGridView3.Rows.Add(order.Symbol, order.OrderId, order.ClientOrderId, order.Price,
                                                    order.ReduceOnly, order.OriginalQuantity, order.CumulativeQuantity, order.ExecutedQuantity, order.CumulativeQuoteQuantity,
                                                    order.Status, order.TimeInForce, order.Type, order.Side,
                                                    order.StopPrice, orderTime,
                                                    updateTime, order.WorkingType);
                    }

                    if (this.dataGridView3.Rows.Count == 0)
                    {
                        MessageBox.Show("You have no orders to display.", "Info", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show(response.Error.Message, "Error.", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void dataGridView3_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged == DataGridViewElementStates.Selected)
            {
                this.textBox2.Text = e.Row.Cells[0].Value.ToString();
                this.textBox3.Text = e.Row.Cells[1].Value.ToString();
            }
            else
            {
                this.textBox2.Text = "";
                this.textBox3.Text = "";
            }
        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (this.textBox3.Text == null ||
               this.textBox3.Text == "")
            {
                MessageBox.Show("Invalid Order ID.", "Error", MessageBoxButtons.OK);
                return;
            }

            try
            {
                long orderId = Convert.ToInt64(this.textBox3.Text);
                var response = RestApi.GetClient().FuturesUsdt.Order.CancelOrder(this.textBox2.Text, orderId);

                var columns = dataGridView3.Columns;

                if (response.Success)
                {
                    // remove row also from table
                    foreach (DataGridViewRow row in this.dataGridView3.Rows)
                    {
                        if (row.Cells[columns["OpenOrderId"].Index].Value.ToString() == this.textBox3.Text)
                        {
                            // close also the position
                            if (checkBox2.Checked)
                            {
                                string symbol = row.Cells[columns["OrderSymbol"].Index].Value.ToString();
                                string qty = row.Cells[columns["OrigQty"].Index].Value.ToString();
                                string clientID = row.Cells[columns["ClientOrderId"].Index].Value.ToString().Split('_')[0];
                                OrderSide side = OrderSide.Buy;
                                if (row.Cells[columns["Side"].Index].Value.ToString().ToLower() == "sell")
                                    side = OrderSide.Sell;
                                RestApi.SendNewOrder(symbol, qty, side, OrderType.Market, true, "0", "0", clientID);
                            }


                            this.dataGridView3.Rows.Remove(row);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(response.Error.ToString(), "Error", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string response = RestApi.SendNewOrder(this.textBox4.Text, this.textBox5.Text, OrderSide.Buy, OrderType.Market, checkBox1.Checked);
            MessageBox.Show(response, "Response", MessageBoxButtons.OK);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string response = RestApi.SendNewOrder(this.textBox4.Text, this.textBox5.Text, OrderSide.Sell, OrderType.Market, checkBox1.Checked);
            MessageBox.Show(response, "Response", MessageBoxButtons.OK);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            // Fix scaling
            dataGridView1.Width = (int)Math.Round(this.Width * 1 / factor.Width);
            dataGridView2.Width = (int)Math.Round(this.Width * 1 / factor.Width);
            dataGridView3.Width = (int)Math.Round(this.Width * 1 / factor.Width);
            dataGridView4.Width = (int)Math.Round(this.Width * 1 / factor.Width);
            richTextBox1.Width = (int)Math.Round(this.Width * 1 / factor.Width);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string response = RestApi.SendNewOrder(this.textBox10.Text, this.textBox11.Text, OrderSide.Buy, OrderType.StopMarket,
                              true, "", this.textBox13.Text);
            MessageBox.Show(response, "Response", MessageBoxButtons.OK);

        }

        private void button12_Click(object sender, EventArgs e)
        {
            string response = RestApi.SendNewOrder(this.textBox10.Text, this.textBox11.Text, OrderSide.Sell, OrderType.StopMarket,
                          true, "", this.textBox13.Text);
            MessageBox.Show(response, "Response", MessageBoxButtons.OK);

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click_1(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {
                dataGridView4.Rows.Clear();

                WebCallResult<IEnumerable<BinancePositionDetailsUsdt>> response = RestApi.GetFuturesAccountPositions();

                if (response.Success)
                {
                    foreach (var position in response.Data)
                    {
                        if ((position.PositionAmount > 0.00001m) || (position.PositionAmount < -0.00001m))
                        {
                            this.dataGridView4.Rows.Add(position.EntryPrice, position.MarginType, position.IsAutoAddMargin,
                                                        position.IsolatedMargin, position.Leverage, position.LiquidationPrice,
                                                        position.MarkPrice, position.MaxNotionalValue, position.PositionAmount,
                                                        position.Symbol, position.UnrealizedProfit);
                        }
                    }

                    if (this.dataGridView4.Rows.Count == 0)
                    {
                        MessageBox.Show("You have no positions to display.", "Info", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show(response.Error.Message, "Error.", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.textBox8.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            button13.Text = "Downloading....";
            button13.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (textBox8.Text == "")
            {
                MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK);
                return;
            }

            bool downloadFutures = true;
            if (radioButton1.Checked)
            {
                downloadFutures = false;
            }

            List<string> symbolsList = new List<string>();
            if (comboSymbol == "ALL")
            {
                symbolsList.AddRange(binanceSymbols);
            }
            else
            {
                symbolsList.Add(comboSymbol);
            }

            foreach (string singleSymbol in symbolsList)
            {
                try
                {
                    FileDownload.DownloadToFileWithRetry(downloadFutures,
                                                            textBox8.Text,
                                                            singleSymbol,
                                                            dateTimePicker1.Value,
                                                            dateTimePicker2.Value,
                                                            separator,
                                                            backgroundWorker1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(singleSymbol + ":" + ex.Message, "Error.", MessageBoxButtons.OK);
                    return;
                }
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0 && e.ProgressPercentage <= 100)
            {
                progressBar1.Value = e.ProgressPercentage;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button13.Text = "Download";
            button13.Enabled = true;
            MessageBox.Show("Download finished.", "Info", MessageBoxButtons.OK);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                separator = ",";
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                separator = ";";
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                separator = "\t";
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string APIkey = this.textBox9.Text;
            string SecretKey = this.textBox12.Text;
            string encryptedSecretKey = "";

            // encrypt SecretKey
            try
            {
                encryptedSecretKey = StringCipher.Encrypt(SecretKey, "bnc123");
            }
            catch (Exception)
            {
                MessageBox.Show("Can't encrypt SecretKey.", "Error", MessageBoxButtons.OK);
            }

            using (StreamWriter writetext = new StreamWriter(".\\System.AppCtx.dll"))
            {
                writetext.WriteLine(APIkey);
                if (encryptedSecretKey == "")
                {
                    writetext.WriteLine("nen");
                    writetext.WriteLine(SecretKey);
                }
                else
                {
                    writetext.WriteLine("en");
                    writetext.WriteLine(encryptedSecretKey);
                }
            }

            RestApi.RestApiDispose();
            RestApi.RestApiConnect(APIkey, SecretKey);
            UpdatePositionMode();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboSymbol = comboBox2.SelectedItem.ToString();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void UpdatePositionMode()
        {
            // Detect if Hedging or Netting
            if (RestApi.IsHedgingEnabled())
            {
                this.Text = this.Text.Replace("Netting", "Hedging");
            }
            else
            {
                this.Text = this.Text.Replace("Hedging", "Netting");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            string symbol = this.textBox7.Text;

            if (symbol == null ||
               symbol == "")
            {
                MessageBox.Show("Invalid symbol.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {

                IEnumerable<BinanceFuturesOrder> orders = null;
                bool response = RestApi.GetAllOrdersMultipleRequests(symbol, dateTimePicker3.Value.Date, ref orders);


                // get positions in MT4 format
                AllPositionsInfo posInfo = RestApi.GetMT4Positions(orders,
                                                                    Convert.ToDecimal(textBox14.Text));
                // Get stats
                AllStrategyStats strategies = new AllStrategyStats(posInfo);

                // Write to .csv file
                bool header = true;
                string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string reportFolder = Path.Combine(currentDir, "Report");
                System.IO.Directory.CreateDirectory(reportFolder);
                using (StreamWriter file = new StreamWriter(Path.Combine(reportFolder, symbol + "_trades.csv"), false))
                {
                    foreach (PositionInfo trade in posInfo.closedPositions)
                    {
                        if (header == true)
                        {
                            file.WriteLine(trade.PropertyValues(true));
                            header = false;
                        }
                        file.WriteLine(trade.PropertyValues());
                    }

                    foreach (PositionInfo trade in posInfo.openPositions)
                        file.WriteLine(trade.PropertyValues());

                    //foreach (PositionInfo trade in posInfo.unclosedPositions)
                    //    file.WriteLine(trade.PropertyValues());
                }

                dataGridView5.Rows.Clear();

                // Calculate Profit factor and display orders
                foreach (StrategyStats strategy in strategies.allStrategiesStats)
                {
                    decimal profitFactor = 1.0m;
                    if (strategy.GrossLoss < -0.000001m)
                        profitFactor = strategy.GrossProfit / ((-1.0m) * (strategy.GrossLoss - strategy.Fees));
                    decimal winPercent = 0.0m;
                    if (strategy.WiningTrades > 0)
                    {
                        winPercent = ((decimal)strategy.WiningTrades / (decimal)strategy.TotalTrades) * 100.0m;
                    }
                    this.dataGridView5.Rows.Add(strategy.StrategyName,
                                                strategy.Profit - strategy.Fees, // net profit
                                                Math.Round(profitFactor, 2),
                                                strategy.TotalTrades,
                                                Math.Round(winPercent, 2),
                                                strategy.WiningTrades,
                                                strategy.LoosingTades,
                                                strategy.TradedVolume,
                                                strategy.Fees,
                                                strategy.Profit,
                                                strategy.GrossProfit,
                                                strategy.GrossLoss,
                                                "");
                }

                if (this.dataGridView5.Rows.Count == 0)
                {
                    MessageBox.Show("You have no orders to display.", "Info", MessageBoxButtons.OK);
                }
                else
                {
                    this.dataGridView5.Sort(this.dataGridView5.Columns[1], ListSortDirection.Descending);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void comboBox2_TextUpdate(object sender, EventArgs e)
        {
            comboSymbol = comboBox2.Text;
        }

        private void OpenOrders_Click(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                List<DataGridViewRow> removedRows = new List<DataGridViewRow>();
                var columns = dataGridView3.Columns;

                // remove row also from table
                foreach (DataGridViewRow row in this.dataGridView3.Rows)
                {
                    string symbol = row.Cells[columns["OrderSymbol"].Index].Value.ToString();
                    string qty = row.Cells[columns["OrigQty"].Index].Value.ToString();
                    string clientID = row.Cells[columns["ClientOrderId"].Index].Value.ToString().Split('_')[0];
                    long orderId = Convert.ToInt64(row.Cells[columns["OpenOrderId"].Index].Value.ToString());
                    OrderSide side = OrderSide.Buy;
                    if (row.Cells[columns["Side"].Index].Value.ToString().ToLower() == "sell")
                        side = OrderSide.Sell;

                    RestApi.GetClient().FuturesUsdt.Order.CancelOrder(symbol, orderId);

                    // close also the position
                    if (checkBox2.Checked)
                    {
                        RestApi.SendNewOrder(symbol, qty, side, OrderType.Market, true, "0", "0", clientID);
                    }
                    removedRows.Add(row);
                }

                foreach (var row in removedRows)
                {
                    this.dataGridView3.Rows.Remove(row);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (!RestApi.IsConnected())
            {
                MessageBox.Show("Please set keys.", "Info", MessageBoxButtons.OK);
                return;
            }

            foreach (string symbol in binanceSymbols)
            {
                bool append = false;
                DateTime startDate = dateTimePicker3.Value.Date;
                long lastTicket = 0;

                // get path
                string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string reportFolder = Path.Combine(currentDir, "Report");
                string fileName = Path.Combine(reportFolder, symbol + "_orders.csv");

                // get last date from file 
                if (File.Exists(fileName))
                {
                    string lastLine = File.ReadLines(fileName).Last();
                    string[] split = lastLine.Split(',');
                    startDate = DateTime.Parse(split[1]);
                    lastTicket = Int64.Parse(split[2]);
                    append = true;
                }

                IEnumerable<BinanceFuturesOrder> orders = Enumerable.Empty<BinanceFuturesOrder>();
                // get positions in MT4 format
                try
                {
                    RestApi.GetAllOrdersMultipleRequests(symbol, startDate, ref orders);
                    orders = orders.OrderBy(c => c.CreatedTime);
                }
                catch (Exception)
                {

                }

                if (orders.Count() == 0)
                {
                    BinanceFuturesOrder order = new BinanceFuturesOrder();
                    order.Symbol = symbol;
                    order.CreatedTime = DateTime.Now.AddDays(-6);
                    order.OrderId = 1;
                    orders = new[] { order };
                }

                // Write to .csv file
                bool header = true;
                string line = "";
                System.IO.Directory.CreateDirectory(reportFolder);
                using (StreamWriter file = new StreamWriter(Path.Combine(reportFolder, symbol + "_orders.csv"), append))
                {
                    foreach (var order in orders)
                    {
                        if (header == true && append == false)
                        {
                            // header
                            line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                                                  "Symbol", "CreatedTime", "OrderId", "ClientOrderId", "AvgPrice",
                                                  "ReduceOnly", "OriginalQuantity", "ExecutedQuantity", "CumulativeQuantity",
                                                  "Status", "TimeInForce", "Type", "Side", "PositionSide",
                                                  "StopPrice", "UpdateTime");
                            file.WriteLine(line);
                            header = false;
                        }
                        if (lastTicket != order.OrderId)
                        {
                            line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                                                  order.Symbol, order.CreatedTime, order.OrderId, order.ClientOrderId, order.AvgPrice,
                                                  order.ReduceOnly, order.OriginalQuantity, order.ExecutedQuantity, order.CumulativeQuantity,
                                                  order.Status, order.TimeInForce, order.Type, order.Side, order.PositionSide,
                                                  order.StopPrice, order.UpdateTime);
                            file.WriteLine(line);
                        }
                    }
                }
            }
            MessageBox.Show("Done", "Info.", MessageBoxButtons.OK);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string reportFolder = Path.Combine(currentDir, "Report");
            string fileName = "";

            try
            {
                foreach (string symbol in binanceSymbols)
                {
                    // get path
                    fileName = Path.Combine(reportFolder, symbol + "_orders.csv");

                    List<BinanceFuturesOrder> orders = new List<BinanceFuturesOrder>();

                    // get orders 
                    if (File.Exists(fileName))
                    {
                        var lines = File.ReadLines(fileName);

                        foreach (string line in lines)
                        {
                            if (!line.Contains("Symbol") &&
                                !line.Contains("CreatedTime") &&
                                !line.Contains("OrderId") )
                            {
                                string[] split = line.Split(',');
                                BinanceFuturesOrder order = new BinanceFuturesOrder();
                                order.Symbol = split[0];
                                order.CreatedTime = DateTime.Parse(split[1]);
                                order.OrderId = Int64.Parse(split[2]);
                                order.ClientOrderId = split[3];
                                order.AvgPrice = Decimal.Parse(split[4]);
                                order.ReduceOnly = Boolean.Parse(split[5]);
                                order.OriginalQuantity = Decimal.Parse(split[6]);
                                order.ExecutedQuantity = Decimal.Parse(split[7]);
                                order.CumulativeQuantity = Decimal.Parse(split[8]);
                                order.Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), split[9]);
                                order.TimeInForce = (TimeInForce)Enum.Parse(typeof(TimeInForce), split[10]);
                                order.Type = (OrderType)Enum.Parse(typeof(OrderType), split[11]);
                                order.Side = (OrderSide)Enum.Parse(typeof(OrderSide), split[12]);
                                order.PositionSide = (PositionSide)Enum.Parse(typeof(PositionSide), split[13]);
                                if (split[14] == "")
                                    split[14] = "0.0";
                                order.StopPrice = Decimal.Parse(split[14]);
                                order.UpdateTime = DateTime.Parse(split[15]);
                                orders.Add(order);
                            }
                        }
                    }

                    // get positions in MT4 format
                    AllPositionsInfo posInfo = RestApi.GetMT4Positions(orders,
                                                                        Convert.ToDecimal(textBox14.Text));
                    if(posInfo.closedPositions.Count() > 0)
                    {
                        // Create trades File
                        bool header = true;
                        System.IO.Directory.CreateDirectory(reportFolder);
                        using (StreamWriter file = new StreamWriter(Path.Combine(reportFolder, symbol + "_trades.csv"), false))
                        {
                            foreach (PositionInfo trade in posInfo.closedPositions)
                            {
                                if (header == true)
                                {
                                    file.WriteLine(trade.PropertyValues(true));
                                    header = false;
                                }
                                file.WriteLine(trade.PropertyValues());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK);
            }

            MessageBox.Show("Done", "Done.", MessageBoxButtons.OK);
        }

        private void textBox7_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (PassTxtBox.Text.Length == 0)
            {
                MessageBox.Show("Please set DB password.", "info", MessageBoxButtons.OK);
            }

            dbTimer.Enabled = true;
            button19.Enabled = false;
            button21.Enabled = true;
        }

        private void UpadateOrdersDb()
        {
            // open db connection
            string sqlConnectionStr = string.Format("datasource={0};port=3306;username=root;password={1}", textBox15.Text,PassTxtBox.Text);
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectionStr);
            sqlConnection.Open();


            DateTime startDate = new DateTime(2021, 01, 01);
            foreach (string symbol in binanceSymbols)
            {
                IEnumerable<BinanceFuturesOrder> orders = Enumerable.Empty<BinanceFuturesOrder>();
                // get positions in MT4 format
                try
                {
                    RestApi.GetAllOrdersMultipleRequests(symbol, startDate, ref orders);
                    orders = orders.OrderBy(c => c.CreatedTime);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error getting orders.", "Error.", MessageBoxButtons.OK);
                }

                // Write to db
                foreach (var order in orders)
                {
                    string sqlInsert = string.Format("insert into rixverschema.app_orders(" +
                                                        "orderID,symbol,createdTime,clientOrderId,avgPrice,reduceOnly," +
                                                        "originalQuantity,executedQuantity,cumulativeQuantity," +
                                                        "status,timeInForce,type,side,positionSide,stopPrice,updateTime) " +
                                                        "values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')",
                                                        order.OrderId, order.Symbol, order.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"), order.ClientOrderId, order.AvgPrice,
                                                        order.ReduceOnly.ToString(), order.OriginalQuantity, order.ExecutedQuantity, order.CumulativeQuantity,
                                                        order.Status.ToString(), order.TimeInForce.ToString(), order.Type.ToString(), order.Side.ToString(), order.PositionSide.ToString(),
                                                        order.StopPrice, order.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    MySqlCommand sqlCommand = new MySqlCommand(sqlInsert, sqlConnection);
                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Duplicate entry"))
                        {
                            MessageBox.Show("Error:" + ex.Message, "Error", MessageBoxButtons.OK);
                            break;
                        }
                    }
                }
                DateTime lastUpdatedTime = DateTime.Now.AddDays(-1);
                if (orders.Count() > 0)
                {
                    lastUpdatedTime = orders.Last().CreatedTime;
                }
                string lastUpdated = string.Format("insert into rixverschema.app_lastorderupdate(symbol,updatedTime) " +
                                   "values('{0}','{1}') " +
                                   "ON DUPLICATE KEY UPDATE updatedTime='{1}'", symbol, lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss"));
                MySqlCommand sqlOrderUpdated = new MySqlCommand(lastUpdated, sqlConnection);
                try
                {
                    sqlOrderUpdated.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK);
                    break;
                }
            }
            sqlConnection.Close();
            MessageBox.Show("Done", "Info.", MessageBoxButtons.OK);
        }

        private void dbTimer_Tick(object sender, EventArgs e)
        {
            UpadateOrdersDb();
        }

        private void label23_Click(object sender, EventArgs e)
        {
        }

        private void button21_Click(object sender, EventArgs e)
        {
            dbTimer.Enabled = false;
            button19.Enabled = true;
            button21.Enabled = false;

        }

        private void button20_Click(object sender, EventArgs e)
        {
            UpadateOrdersDb();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                textBox8.Text = "C:\\History\\Spot";
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                textBox8.Text = "C:\\History\\Futures";
            }
        }
    }
}
