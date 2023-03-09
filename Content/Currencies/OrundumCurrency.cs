using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace ArknightsMod.Content.Currencies
{
	public class OrundumCurrency : CustomCurrencySingleCoin
	{
		public OrundumCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
		{
			this.CurrencyTextKey = CurrencyTextKey;
		}
	}
}