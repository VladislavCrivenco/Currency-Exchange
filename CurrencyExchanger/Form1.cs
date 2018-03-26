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
using PR_Lab2.Currencies;

namespace PR_Lab2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            InitCurrencySources();
            await InitCurrencyPickers();
        }

        private async Task InitCurrencyPickers()
        {
            currentSellCurrency = await currentCurrencySource.GetCurrency(Currency.UsdCode);
            label13.Text = currentSellCurrency.Cod;


            currentBuyCurrency = await currentCurrencySource.GetCurrency(Currency.MdlCode);
            label17.Text = currentBuyCurrency.Cod;
            //currentBuyCurrency = new Currency();
            //currentSellCurrency = new Currency();

            if (currentSellCurrency == null || currentBuyCurrency == null)
            {
                Debug.WriteLine(currentBuyCurrency + " \n" + currentSellCurrency);

                throw new ArgumentException("No default currency found");
            }

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

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }


        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            currentDateSelected = dateTimePicker.Value;
        }

        private async void currency_sell_label_click(object sender, EventArgs e)
        {
            var label = sender as Label;

            ////invert currencies
            //if (label.Text == currentBuyCurrencyLabel.Text)
            //{
            //    var oldValue = currentSellCurrencyLabel.Text;
            //    currentBuyCurrency = await currentCurrencySource.GetCurrency(oldValue);
            //    currentBuyCurrencyLabel.Text = currentBuyCurrency.Cod;
            //}

            currentSellCurrency = await currentCurrencySource.GetCurrency(label.Text);
            label13.Text = currentSellCurrency.Cod;

            currentSellCurrencyLabel.Text = currentSellCurrency.Cod;


            //raise onTextChangedEvent
            var currenValue = SellValueTextBox.Text;
            SellValueTextBox.Text = "";
            SellValueTextBox.Text = currenValue;
        }

        private async void currency_buy_label_click(object sender, EventArgs e)
        {
            var label = sender as Label;

            ////invert currencies
            //if (label.Text == currentSellCurrencyLabel.Text)
            //{
            //    var oldValue = currentBuyCurrencyLabel.Text;
            //    currentSellCurrency = await currentCurrencySource.GetCurrency(oldValue);
            //    currentSellCurrencyLabel.Text = currentSellCurrency.Cod;
            //}

            currentBuyCurrency = await currentCurrencySource.GetCurrency(label.Text);
            label17.Text = currentBuyCurrency.Cod;
            currentBuyCurrencyLabel.Text = currentBuyCurrency.Cod;

            //raise onTextChangedEvent
            var currenValue = BuyValueTextBox.Text;
            BuyValueTextBox.Text = "";
            BuyValueTextBox.Text = currenValue;
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

            var resultValue = sellvalue * currentSellCurrency.Value * currentSellCurrency.Nominal;
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
            if (currentSellCurrency.Cod == Currency.MdlCode)
            {
                var resultValue = buyValue * currentBuyCurrency.Value * currentBuyCurrency.Nominal;
                SellValueTextBox.Text = resultValue.ToString("0.0000");
            }
            else
            {
                var resultValue = buyValue / currentSellCurrency.Value / currentSellCurrency.Nominal;
                SellValueTextBox.Text = resultValue.ToString("0.0000");
            }
        }
    }
}

