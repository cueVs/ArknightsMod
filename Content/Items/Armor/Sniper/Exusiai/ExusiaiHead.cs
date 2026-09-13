using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Exusiai
{
	[AutoloadEquip(EquipType.Head)]
	public class ExusiaiHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 168,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Exusiai",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<SugarLump>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Exusiai.SetBonus",
		};

		// 时装效果：戴着就发光。
		public override void UpdateVanityEquip(Player player) {
			Lighting.AddLight(player.Center, new Vector3(1f, 1f, 1f));
		}

		// 头顶光环叠加层，走文档 12.5 的【路 A】。
		internal class ExusiaiHeadLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.head 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<ExusiaiHead>(player, EquipType.Head) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Sniper/Exusiai/ExusiaiHead_Ring").Value;

				var offset = new Vector2(1, -3) + new Vector2(0, -26);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
			}
		}
	}
}
