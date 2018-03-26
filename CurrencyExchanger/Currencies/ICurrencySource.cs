using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICurrencySource
{
    string GetSourceDescription();
    string GetSourceShortDescription();
    Task<List<Currency>> GetCurrencies();
    Task<Currency> GetCurrency(string key);
}