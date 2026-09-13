using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.CivilightEterna
{
	[AutoloadEquip(EquipType.Head)]
	public class CivilightEternaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 193,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.CivilightEterna",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<NucleicCrystalSinter>(6)
				.AddIngredient<SolidifiedFiberBoard>(4),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.CivilightEterna.SetBonus",
		};
	}

	// 后发叠加层。整张贴图直接画、不按帧取，走文档 12.5 的【路 A】用 PlayerLayerHelper。
	internal class CivilightEternaHeadLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
			// 不比对 player.head 槽位 ID——原因见 IsPartVisible 的注释。
			return NeoArmorReforgeSetLoader.IsPartVisible<CivilightEternaHead>(player, EquipType.Head) && !player.dead;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Texture2D texture = ModContent.Request<Texture2D>
				("ArknightsMod/Content/Items/Armor/Supporter/CivilightEterna/CivilightEternaHead_BackHair").Value;

			var offset = new Vector2(0, -3);
			PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
		}
	}
}
