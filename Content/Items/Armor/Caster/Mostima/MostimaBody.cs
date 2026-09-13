using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Mostima
{
	[AutoloadEquip(EquipType.Body)]
	public class MostimaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 10,
			LifeBonus = 92,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mostima",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<GrindstonePentahydrate>(6),
		};

		internal class MostimaWingLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.body 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<MostimaBody>(player, EquipType.Body) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Caster/Mostima/MostimaBody_Wing").Value;

				var offset = new Vector2(1, -3) + new Vector2(-2, 8);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 1, offset);
			}
		}
	}
}
