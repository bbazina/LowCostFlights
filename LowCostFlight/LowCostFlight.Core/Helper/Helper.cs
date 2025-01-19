using LowCostFlight.Domain.Models;

namespace LowCostFlight.Core.Helper
{
    public static class Helper
    {
        public static string GetCurrency(this Currency currency)
        {
            return currency switch
            {
                Currency.EUR => "EUR",
                Currency.USD => "USD",
                Currency.GBP => "GBP",
                _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null),
            };
        }
    }
}
