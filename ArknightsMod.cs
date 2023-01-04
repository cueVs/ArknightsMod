using ArknightsMod.Content.NPCs.Friendly;
using Terraria.ModLoader;
using Terraria.GameContent.UI;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
        public static int OrundumCurrencyId;
        internal Closure.AOSystem CurrentAO;

        public override void Load()
        {
            // Registers a new custom currency
            OrundumCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OrundumCurrency(ModContent.ItemType<Content.Items.Orundum>(), 9999L, "Mods.ArknightsMod.Currencies.OrundumCurrency"));
        }
    }
}