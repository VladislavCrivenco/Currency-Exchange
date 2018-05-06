using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PR_Lab2;

namespace CurrencyExchanger.Currencies
{
    public class BNMCurrencySource : ICurrencySource
    {
        private const string BankApi = "https://www.bnm.md/ro/official_exchange_rates";
        //"https://www.bnm.md/ro/official_exchange_rates?get_xml=1&date=25.03.2018";
        //http://www.curs.md/ru/csv_graph_provider?currency[]=EUR&currency_rel=MDL&date_end=22.04.2018&date_start=08.04.2018&bank=bnm
        //http://www.curs.md/ru/csv_graph_provider?currency[]=EUR&currency_rel=MDL&date_end=22.04.2018&date_start=04.04.2018&bank=mobiasbanca

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
            return "Banca Nationala a Moldovei";
        }

        public string GetSourceShortDescription()
        {
            return "BNM";
        }

        public async Task<Currency> GetCurrency(string code, DateTime date)
        {
            var list = await GetCurrencies(date);
            return list.Where(x => x.Cod.ToLower().Equals(code.ToLower())).FirstOrDefault();
        }

        private async Task<List<Currency>> GetRemoteCurrencies(DateTime date)
        {
            var response = await HttpUtil.PerformGetRequest(BankApi,
                new Dictionary<string, string>
                {
                    ["get_xml"] = "1",
                    ["date"] = date.ToShortDateString()
                });

            if (response.status == HttpStatusCode.OK)
            {
                return DeserializeResponse(response.data);
            }
            else
            {
                return null;
            }
        }

        private List<Currency> DeserializeResponse(string xml)
        {
            if (xml.IsEmpty())
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(BNMCurrencyModel));
            BNMCurrencyModel model = null;
            using (var reader = new StringReader(xml))
            {
                model = (BNMCurrencyModel)serializer.Deserialize(reader);
                return model.ToCurrencyList();
            }
        }


        [XmlRoot("ValCurs")]
        public class BNMCurrencyModel
        {
            [XmlElement("Valute")]
            public List<BNMCurrency> Currencies { get; set; }

            public List<Currency> ToCurrencyList()
            {
                var result = new List<Currency>();
                foreach (var BNMCurrency in Currencies)
                {
                    var currency = new Currency
                    {
                        Name = BNMCurrency.Name,
                        Nominal = BNMCurrency.Nominal,
                        Value = BNMCurrency.Value,
                        Cod = BNMCurrency.Code
                    };

                    result.Add(currency);
                }

                return result;
            }
        }

        public class BNMCurrency
        {
            [XmlElement("NumCode")]
            public int NumCode { get; set; }

            [XmlElement("CharCode")]
            public string Code { get; set; }

            [XmlElement("Nominal")]
            public int Nominal { get; set; }

            [XmlElement("Name")]
            public string Name { get; set; }

            [XmlElement("Value")]
            public decimal Value { get; set; }
        }
    }
}