using Terraria.GameContent.UI;

namespace ArknightsMod.Content.Currencies
{
	public class OrundumCurrency : CustomCurrencySingleCoin
	{
		public OrundumCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap) {
			this.CurrencyTextKey = CurrencyTextKey;
		}
	}
}