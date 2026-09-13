using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	[AutoloadEquip(EquipType.Head)]
	public class TyphonHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 170,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Typhon",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<OrirockConcentration>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Typhon.SetBonus",
		};

		// 犄角溢出层：只在特定几帧（TyphonVanityAnim.HornOverlayAtlasRows）额外画一张贴图，
		// 补上超出头部帧表范围的角。挂在 Head 之后，盖在头部贴图上。
		//
		// 迁移改动：旧代码为了判断"是不是穿着提丰头饰"，自己缓存了一个 HeadEquipSlot 静态
		// 字段，还在 Load / SetStaticDefaultsNoServer / SetVanityDefaults 三处各赋值一次，
		// 判定时先比物品 ItemID（armor[0]/armor[10]）、再退回比 player.head 槽位 ID。
		// 这套东西迁移后全都不需要了：
		//   · 比物品 ItemID 会漏掉套装形态——套装件是另一个独立的 ItemID；
		//   · 比 player.head 槽位 ID 会被 Player.SetMatch 静默打断（见 IsPartVisible 注释）。
		// 统一换成 IsPartVisible，时装和套装都认，那个静态字段和三处赋值一并删掉。
		internal class TyphonHeadOverflowLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				return !player.dead
					&& NeoArmorReforgeSetLoader.IsPartVisible<TyphonHead>(player, EquipType.Head)
					&& TyphonVanityAnim.BodyFrameMatchesHornsLongStrip(player);
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Player p = drawInfo.drawPlayer;

				Texture2D extra = ModContent.Request<Texture2D>(
					"ArknightsMod/Content/Items/Armor/Sniper/Typhon/TyphonHead_Horns").Value;
				if (extra.Width <= 0 || extra.Height <= 0)
					return;

				Rectangle bodyFrame3 = p.bodyFrame;
				Vector2 headVect2 = drawInfo.headVect;
				if (p.gravDir == 1f) {
					bodyFrame3.Height -= 4;
				}
				else {
					headVect2.Y -= 4f;
					bodyFrame3.Height -= 4;
				}

				if (bodyFrame3.Width <= 0 || bodyFrame3.Height <= 0)
					return;

				Vector2 basePos = new(
					(int)(drawInfo.Position.X - Main.screenPosition.X - (p.bodyFrame.Width / 2) + (p.width / 2)),
					(int)(drawInfo.Position.Y - Main.screenPosition.Y + p.height - p.bodyFrame.Height + 4f));
				Vector2 helmetDrawPos = drawInfo.helmetOffset + basePos + p.headPosition + drawInfo.headVect;
				Vector2 topCenter = helmetDrawPos + new Vector2(
					-headVect2.X + bodyFrame3.Width * 0.5f,
					-headVect2.Y);
				Vector2 origin = new(extra.Width * 0.5f, extra.Height);

				drawInfo.DrawDataCache.Add(
					new DrawData(extra, topCenter, null, drawInfo.colorArmorHead, p.headRotation, origin, 1f, drawInfo.playerEffect, 0) {
						shader = drawInfo.cHead
					});
			}
		}
	}
}
