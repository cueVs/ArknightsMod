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
	[AutoloadEquip(EquipType.Body)]
	public class ExusiaiAlterBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 118,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.ExusiaiAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				// .AddIngredient<环烃预制体>(5) 材料缺失！（旧代码里就是注释掉的，原样保留）
				.AddIngredient<PolymerizationPreparation>(6),
		};

		// 背部翅膀叠加层，走文档 12.5 的【路 A】。第三个参数 1 = 躯干部位（决定用哪个染料槽）。
		internal class ExusiaiAlterWingLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				return NeoArmorReforgeSetLoader.IsPartVisible<ExusiaiAlterBody>(player, EquipType.Body) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Specialist/ExusiaiAlter/ExusiaiAlter_Wings").Value;

				var offset = new Vector2(1, -3) + new Vector2(-2, 8);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 1, offset);
			}
		}
	}
}
