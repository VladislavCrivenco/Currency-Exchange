using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PR_Lab2;

namespace CurrencyExchanger.Currencies
{
    class MAIBCurrencySource : ICurrencySource
    {
        private static string _urlFormat = "http://www.curs.md/ru/csv_graph_provider?currency[]=USD&currency[]=EUR&currency[]=RUB&currency[]=RON&currency[]=UAH&currency[]=GBP&currency_rel=MDL&date_end={0}&date_start={0}&bank=maib";

        private Dictionary<DateTime, List<Currency>> history = new Dictionary<DateTime, List<Currency>>();

        public async Task<List<Currency>> GetCurrencies(DateTime date)
        {
            if (!history.ContainsKey(date.Date))
            {
                var list = await GetRemoteCurrencies(date);
                list.Add(new Currency
                {
                    Cod = "MDL",
                    Name = "Leu Moldovenesc",
                    Nominal = 1,
                    Value = 1
                });
                history.Add(date.Date, list);
                return list;
            }
            else
            {
                return history[date.Date];
            }
        }

        public string GetSourceDescription()
        {
            return "Moldova AgroindBank";
        }

        public string GetSourceShortDescription()
        {
            return "MAIB";
        }

        public async Task<Currency> GetCurrency(string code, DateTime date)
        {
            var list = await GetCurrencies(date);
            return list.Where(x => x.Cod.ToLower().Equals(code.ToLower())).FirstOrDefault();
        }

        private async Task<List<Currency>> GetRemoteCurrencies(DateTime date)
        {
            var response = await HttpUtil.PerformGetRequest(String.Format(_urlFormat, date.ToShortDateString()));
            if (response.status == HttpStatusCode.OK)
            {
                return DeserializeResponse(response.data);
            }
            else
            {
                return null;
            }
        }

        private List<Currency> DeserializeResponse(string csv)
        {
            var lines = csv.Split('\n');
            if (lines.Length <= 1)
            {
                return null;
            }

            var currencies = lines[1].Split(';');
            var list = new List<Currency>();

            list.Add(new Currency
            {
                Cod = Currency.UsdCode,
                Name = "US Dollar",
                Nominal = 1,
                Value = Decimal.Parse(currencies[2])
            });

            list.Add(new Currency
            {
                Cod = Currency.EurCode,
                Name = "Euro",
                Nominal = 1,
                Value = Decimal.Parse(currencies[4])
            });


            list.Add(new Currency
            {
                Cod = Currency.RubCode,
                Name = "Russian Ruble",
                Nominal = 1,
                Value = Decimal.Parse(currencies[6])
            });


            list.Add(new Currency
            {
                Cod = Currency.RonCode,
                Name = "Romanian Leu",
                Nominal = 1,
                Value = Decimal.Parse(currencies[8])
            });


            return list;
        }

    }

}