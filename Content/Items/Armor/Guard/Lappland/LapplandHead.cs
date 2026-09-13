using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Lappland
{
	[AutoloadEquip(EquipType.Head)]
	public class LapplandHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 235,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Lappland",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OptimizedDevice>(6)
				.AddIngredient<OrironCluster>(10),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Lappland.SetBonus",
		};

		// 后发叠加层，走文档 12.5 的【路 A】。
		internal class LapplandHeadLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.head 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<LapplandHead>(player, EquipType.Head) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Guard/Lappland/Lappland_BackHair").Value;

				var offset = new Vector2(0, -3);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
			}
		}
	}
}
