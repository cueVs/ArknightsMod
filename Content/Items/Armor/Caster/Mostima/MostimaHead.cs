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
	[AutoloadEquip(EquipType.Head)]
	public class MostimaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 183,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mostima",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<GrindstonePentahydrate>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Mostima.SetBonus",
		};

		internal class MostimaHeadLayer : PlayerDrawLayer
		{
			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.head 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<MostimaHead>(player, EquipType.Head) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				var texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Caster/Mostima/MostimaHead_Ring").Value;
				var offset = new Vector2(0, -3) + new Vector2(2, -28);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
			}

			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);
		}
	}
}
