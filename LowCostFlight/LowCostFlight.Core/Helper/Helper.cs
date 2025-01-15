using LowCostFlight.Domain.Models;

namespace LowCostFlight.Core.Helper
{
    public static class Helper
    {
        public static string GetCurrency(this Currency currency)
        {
            switch (currency)
            {
                case Currency.EUR:
                    return "EUR";
                case Currency.USD:
                    return "USD";
                case Currency.HRK:
                    return "HRK";
                default:
                    throw new ArgumentOutOfRangeException(nameof(currency), currency, null);
            }
        }
    }
}
