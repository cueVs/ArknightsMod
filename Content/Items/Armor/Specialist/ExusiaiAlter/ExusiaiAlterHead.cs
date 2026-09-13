using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.ExusiaiAlter
{
	[AutoloadEquip(EquipType.Head)]
	public class ExusiaiAlterHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 235,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.ExusiaiAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				// .AddIngredient<重相位对映体>(6) 材料缺失！（旧代码里就是注释掉的，原样保留）
				.AddIngredient<KetonColloid>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.ExusiaiAlter.SetBonus",
		};

		// 时装效果：戴着就发光。新基类的钩子名一样，直接保留。
		public override void UpdateVanityEquip(Player player) {
			Lighting.AddLight(player.Center, new Vector3(1f, 1f, 1f));
		}
	}

	// 头顶光环叠加层，走文档 12.5 的【路 A】。
	internal class ExusiaiAlterHeadLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			return NeoArmorReforgeSetLoader.IsPartVisible<ExusiaiAlterHead>(player, EquipType.Head) && !player.dead;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Texture2D texture = ModContent.Request<Texture2D>
				("ArknightsMod/Content/Items/Armor/Specialist/ExusiaiAlter/ExusiaiAlter_Ring").Value;

			var offset = new Vector2(0, -4) + new Vector2(0, -26);
			PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
		}
	}
}
