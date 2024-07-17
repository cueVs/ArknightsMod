using ArknightsMod.Content.NPCs.Friendly;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		internal Closure.AOSystem CurrentAO;
		public static Effect IACTSW;//冲击波涟漪效果shader（如IACT）
		public static Effect AACTTP;//缩小效果shader（AACT传送）
		public static Effect AACTOC;//变色效果shader（AACT）
		public static Effect AACTOC2;//反色效果shader（AACT转阶段）
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
				AACTTP = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTTP", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTTP"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTTP), "AACTTP"), EffectPriority.VeryHigh);
				Filters.Scene["AACTTP"].Load();
				AACTOC = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTOC"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTOC), "AACTOC"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC"].Load();
				AACTOC2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTOC2"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTOC2), "AACTOC2"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC2"].Load();
			}
		}
	}
}
