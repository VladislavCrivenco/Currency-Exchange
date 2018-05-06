using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchanger.Currencies;

namespace PR_Lab2.Currencies
{
    class CurrencySourceList
    {
        public static List<ICurrencySource> GetSources()
        {
            return new List<ICurrencySource>
            {
                new BNMCurrencySource(),
                new MAIBCurrencySource()
            };
        }
    }
}
