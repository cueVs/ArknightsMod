using ArknightsMod.Content.NPCs.Friendly;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using System;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		internal Closure.AOSystem CurrentAO;
		public const string noTexture = "ArknightsMod/Assets/null";//空材质
		public static Effect IACTSW;//冲击波涟漪效果shader（如IACT）
		public static Effect AACTTP;//缩小效果shader（AACT传送）
		public static Effect AACTOC;//变色效果shader（AACT）
		public static Effect AACTOC2;//反色效果shader（AACT转阶段）
		public static Effect LightRing;//光环shader（AACT二阶段）
		public static Effect CollapsedExplosionPart1;//坍缩爆炸效果（内核）（AACT二阶段）
		public static Effect CollapsedExplosionPart2;//坍缩爆炸效果（描边）（AACT二阶段）
		public static Effect AACTSTG3RBFence;//红蓝光栅效果（AACT三阶段）
		public static Effect AACTSTG3RBNoise;//红蓝噪声效果（AACT三阶段）
		public static Effect FNTwistedRing;//霜星限制阈（扭曲环效果）


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

				LightRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/LightRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["LightRing"] = new Filter(new ScreenShaderData(new Ref<Effect>(LightRing), "LightRing"), EffectPriority.VeryHigh);
				Filters.Scene["LightRing"].Load();

				CollapsedExplosionPart1 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart1", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["CollapsedExplosionPart1"] = new Filter(new ScreenShaderData(new Ref<Effect>(CollapsedExplosionPart1), "CollapsedExplosionPart1"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart1"].Load();

				CollapsedExplosionPart2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["CollapsedExplosionPart2"] = new Filter(new ScreenShaderData(new Ref<Effect>(CollapsedExplosionPart2), "CollapsedExplosionPart2"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart2"].Load();

				AACTSTG3RBFence = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBFence", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTSTG3RBFence"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTSTG3RBFence), "AACTSTG3RBFence"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBFence"].Load();

				AACTSTG3RBNoise = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBNoise", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTSTG3RBNoise"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTSTG3RBNoise), "AACTSTG3RBNoise"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBNoise"].Load();

				FNTwistedRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/FNTwistedRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["FNTwistedRing"] = new Filter(new ScreenShaderData(new Ref<Effect>(FNTwistedRing), "FNTwistedRing"), EffectPriority.VeryHigh);
				Filters.Scene["FNTwistedRing"].Load();
			}
		}
	}
}
