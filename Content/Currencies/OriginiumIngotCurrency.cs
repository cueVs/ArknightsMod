using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace ArknightsMod.Content.Currencies
{
	public class OriginiumIngotCurrency : CustomCurrencySingleCoin
	{
		public OriginiumIngotCurrency(int coinItemID, long currencyCap) : base(coinItemID, currencyCap) {
			CurrencyTextKey = $"[i:{coinItemID}]";
			CurrencyTextColor = Color.Lime;
		}

		public override void GetPriceText(string[] lines, ref int currentLine, long price) {
			Color color = CurrencyTextColor * (Main.mouseTextColor / 255f);
			lines[currentLine++] = $"[c/{color.R:X2}{color.G:X2}{color.B:X2}:{Lang.tip[50].Value} {price}]{Language.GetTextValue(CurrencyTextKey).ToLower()}";
		}

		public override void GetItemExpectedPrice(Item item, out long calcForSelling, out long calcForBuying) {
			calcForSelling = 0;
			calcForBuying = item.value / 50000;
			if (calcForBuying == 0 && item.value > 0)
				calcForBuying = 1;
		}
	}
}
