using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Wisadel
{
	[AutoloadEquip(EquipType.Body)]
	public class WisadelBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 20,
			LifeBonus = 95,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Wisadel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<TransmutedSaltAgglomerate>(4),
		};

		// 旧代码写在 SetStaticDefaultsNoServer 里。新基类的 SetStaticDefaults/SetDefaults
		// 都是 sealed（框架要统一设时装属性），留给子类的入口就是这个 SetVanityDefaults。
		// hasVanityEffects 是 Item 的实例字段，在 SetDefaults 阶段设同样有效。
		public override void SetVanityDefaults() {
			Item.hasVanityEffects = true;
		}

		// 背部翅膀叠加层，走文档 12.5 的【路 A】。第三个参数 1 = 躯干部位（决定用哪个染料槽）。
		internal class WisadelWingLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				return NeoArmorReforgeSetLoader.IsPartVisible<WisadelBody>(player, EquipType.Body) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Sniper/Wisadel/WisadelBody_Back").Value;

				var offset = new Vector2(1, -3);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 1, offset);
			}
		}
	}
}
