using ArknightsMod.Content.NPCs.Friendly;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		internal Closure.AOSystem CurrentAO;
		public static Effect IACTSW;

		public override void Load()
		{
			// Registers a new custom currency
			OrundumCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OrundumCurrency(ModContent.ItemType<Content.Items.Orundum>(), 9999L, "Mods.ArknightsMod.Currencies.OrundumCurrency"));
			//shader
			if (Main.netMode != NetmodeID.Server)
			{
				IACTSW = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/IACTSW", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["IACTSW"] = new Filter(new ScreenShaderData(new Ref<Effect>(IACTSW), "IACTSW"), EffectPriority.VeryHigh);
				Filters.Scene["IACTSW"].Load();
			}
		}
	}
}