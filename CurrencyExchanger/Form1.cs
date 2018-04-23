using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using PR_Lab2.Currencies;

namespace PR_Lab2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            try
            {

                InitializeComponent();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                currentDateSelected = DateTime.Now;
                InitCurrencySources();
                await InitCurrencyPickers();

                await InitHistoryChar(Currency.EurCode, DateTime.Now.Subtract(TimeSpan.FromDays(7)));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private async Task InitCurrencyPickers()
        {
            currentSellCurrency = await currentCurrencySource.GetCurrency(Currency.UsdCode, currentDateSelected);
            label13.Text = currentSellCurrency.Cod;

            currentBuyCurrency = await currentCurrencySource.GetCurrency(Currency.MdlCode, currentDateSelected);
            label17.Text = currentBuyCurrency.Cod;

            if (currentSellCurrency == null || currentBuyCurrency == null)
            {
                Debug.WriteLine(currentBuyCurrency + " \n" + currentSellCurrency);

                throw new ArgumentException("No default currency found");
            }

            var currencies = await currentCurrencySource.GetCurrencies(currentDateSelected);
            comboBox1.Items.AddRange(currencies.ToArray());
            comboBox1.SelectedText = Currency.UsdCode;

            comboBox2.Items.AddRange(currencies.ToArray());
            comboBox2.SelectedText = Currency.MdlCode;



            this.SellValueTextBox.Text = "1";
        }

        private void InitCurrencySources()
        {
            var sources = CurrencySourceList.GetSources();
            if (sources == null || sources.Count == 0)
            {
                throw new ArgumentException("Cannot load without currency source");
            }

            BankSelector.Items.AddRange(sources.Select(x => x.GetSourceDescription()).ToArray());
            BankSelector.SelectedIndex = 0;
            currentCurrencySource = sources[0];
        }

        private async Task InitHistoryChar(string currencyCode, DateTime finalDate)
        {
            //finalDate =  DateTime.Now - TimeSpan.FromDays(47);
            series1.Points.Clear();
            title1.Text =
                $"Dinamica pentru perioada {finalDate.ToShortDateString()} - {DateTime.Today.ToShortDateString()}";
            var data = new List<(DateTime, decimal)>();
            var currentDate = DateTime.Now;
            while (currentDate > finalDate) {
                var currency = await currentCurrencySource.GetCurrency(currencyCode, currentDate);
                data.Add((currentDate, currency.Value));

                currentDate = currentDate.Subtract(TimeSpan.FromDays(1));
            }

            var min = data.Min((x) => x.Item2);
            var max = data.Max((x) => x.Item2);

            var delta = (max - min) / data.Count;
            if (delta < 0.1m)
            {
                delta = 0.1m;
            }
            var interval = delta;

            chartArea1.AxisY.Interval = (double) interval;
            chartArea1.AxisY.Minimum = (double) (min - interval);
            chartArea1.AxisY.Maximum = (double) (max + interval);
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.LabelStyle.Enabled = false;
            series1.Name = currencyCode;

            foreach (var point in data)
            {
                //series1.Points.AddXY(point.Item1.ToShortDateString(), point.Item2);
                series1.Points.Add(new DataPoint
                {

                    ToolTip = point.Item1.ToShortDateString(),
                    LabelToolTip = point.Item1.ToShortDateString(),
                    XValue = point.Item1.Ticks,
                    YValues = new double[] {(double) point.Item2}
                });
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private async void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            currentDateSelected = historyChartDatePicker.Value;

            historyChartDatePicker.Enabled = false;
            await InitHistoryChar(Currency.EurCode, currentDateSelected);
            historyChartDatePicker.Enabled = true;
        }

        private async void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            currentDateSelected = dateTimePicker.Value;
            await UpdateCurrentCurrencies();
            UpdateCursLabel();
        }

        private async void currency_sell_label_click(object sender, EventArgs e)
        {
            var label = sender as Label;

            //invert currencies
            if (label.Text == currentBuyCurrencyLabel.Text)
            {
                var oldValue = currentSellCurrencyLabel.Text;
                currentBuyCurrency = await currentCurrencySource.GetCurrency(oldValue, currentDateSelected);
                currentBuyCurrencyLabel.Text = currentBuyCurrency.Cod;
            }

            currentSellCurrency = await currentCurrencySource.GetCurrency(label.Text, currentDateSelected);
            label13.Text = currentSellCurrency.Cod;

            currentSellCurrencyLabel.Text = currentSellCurrency.Cod;

            //raise onTextChangedEvent
            UpdateSellValueTexBox();

            UpdateCursLabel();
        }

        private async void currency_buy_label_click(object sender, EventArgs e)
        {
            var label = sender as Label;

            //invert currencies
            if (label.Text == currentSellCurrencyLabel.Text)
            {
                var oldValue = currentBuyCurrencyLabel.Text;
                currentSellCurrency = await currentCurrencySource.GetCurrency(oldValue, currentDateSelected);
                currentSellCurrencyLabel.Text = currentSellCurrency.Cod;
            }

            currentBuyCurrency = await currentCurrencySource.GetCurrency(label.Text, currentDateSelected);
            label17.Text = currentBuyCurrency.Cod;
            currentBuyCurrencyLabel.Text = currentBuyCurrency.Cod;

            UpdateBuyValueTexBox();

            UpdateCursLabel();
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            //Just don't let them type commas 
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf(',') > -1)
            {
                e.Handled = true;
            }
        }

        private void UpdateSellValueTexBox()
        {
            var currenValue = SellValueTextBox.Text;
            SellValueTextBox.Text = "";
            SellValueTextBox.Text = currenValue;
        }

        private void UpdateBuyValueTexBox()
        {
            var currenValue = BuyValueTextBox.Text;
            BuyValueTextBox.Text = "";
            BuyValueTextBox.Text = currenValue;
        }

        private void UpdateCursLabel()
        {

            string curs = "1 " + currentSellCurrencyLabel.Text + " = " + currentBuyCurrencyLabel.Text + " " +
                          (currentSellCurrency.Value / currentBuyCurrency.Value).ToString("0.0000");

            label14.Text = curs;
        }

        private async Task UpdateCurrentCurrencies()
        {
            currentSellCurrency = await currentCurrencySource.GetCurrency(currentSellCurrency.Cod, currentDateSelected);

            currentBuyCurrency = await currentCurrencySource.GetCurrency(currentBuyCurrency.Cod, currentDateSelected);
        }

        private void SellValueTextBox_TextChanged(object sender, EventArgs e)
        {
            var stringValue = SellValueTextBox.Text;
            if (stringValue.IsEmpty())
            {
                SellValueTextBox.Text = "";
                BuyValueTextBox.Text = "";
                return;
            }
            if (stringValue.EndsWith(",") || stringValue.IsEmpty())
            {
                return;
            }

            if (BuyValueTextBoxChanged)
            {
                BuyValueTextBoxChanged = false;
                return;
            }

            SellValueTextBoxChanged = true;

            var sellvalue = Decimal.Parse(SellValueTextBox.Text);

            var resultValue = sellvalue * (currentSellCurrency.Value / currentBuyCurrency.Value);
            BuyValueTextBox.Text = resultValue.ToString("0.0000");
        }

        private void BuyValueTextBox_TextChanged(object sender, EventArgs e)
        {
            var stringValue = BuyValueTextBox.Text;
            if (stringValue.IsEmpty())
            {
                SellValueTextBox.Text = "";
                BuyValueTextBox.Text = "";
                return;
            }
            if (stringValue.EndsWith(",") || stringValue.IsEmpty())
            {
                return;
            }
            if (SellValueTextBoxChanged)
            {
                SellValueTextBoxChanged = false;
                return;
            }

            BuyValueTextBoxChanged = true;

            var buyValue = Decimal.Parse(BuyValueTextBox.Text);

            var resultValue = buyValue * (currentBuyCurrency.Value / currentSellCurrency.Value);
            SellValueTextBox.Text = resultValue.ToString("0.0000");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currency = (sender as ComboBox).SelectedItem as Currency;
            currentSellCurrency = currency;
            SellValueTextBox.Text = "1";
            SellValueTextBox_TextChanged(null, null);

            currentSellCurrencyLabel.Text = currentSellCurrency.Cod;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currency = (sender as ComboBox).SelectedItem as Currency;
            currentBuyCurrency = currency;
            BuyValueTextBox.Text = "1";
            BuyValueTextBox_TextChanged(null, null);

            currentBuyCurrencyLabel.Text = currentBuyCurrency.Cod;

        }

        private void chart_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            //Check selected chart element is a data point and set tooltip text
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                //Get selected data point
                DataPoint dataPoint = (DataPoint) e.HitTestResult.Object;

                if (series1.Points.Contains(dataPoint))
                {
                    var ticks = (long)dataPoint.XValue;
                    e.Text =  new DateTime(ticks).ToShortDateString() + ": " + dataPoint.YValues[0];
                }

            }

        }
    }
}

