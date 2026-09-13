using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Provence
{
	[AutoloadEquip(EquipType.Body)]
	public class ProvenceBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 17,
			LifeBonus = 84,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Provence",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<ManganeseTrihydrate>(3)
				.AddIngredient<IntegratedDevice>(2),
		};

		// 尾巴叠加层。这一层**按帧取图**（每帧 18px 高、行距 56px），所以不能用
		// PlayerLayerHelper，得自己写 DrawData —— 属于文档 12.5 的【路 B】。
		// 坐标沿用旧实现（以 MountedCenter 为锚点、按 gravDir/direction 翻转），
		// 迁移只改了可见性判定，绘制逻辑一行未动。
		internal class ProvenceBodyLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.body 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<ProvenceBody>(player, EquipType.Body) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Sniper/Provence/ProvenceBody_Tail").Value;

				var offset = new Vector2(-7, 12);
				int drawX = (int)(drawInfo.drawPlayer.MountedCenter.X + offset.X * drawInfo.drawPlayer.direction - Main.screenPosition.X);
				int drawY = (int)(drawInfo.drawPlayer.MountedCenter.Y + (int)drawInfo.drawPlayer.gravDir * offset.Y - Main.screenPosition.Y);
				int dyeShader = drawInfo.drawPlayer.dye?[0].dye ?? 0;

				float offsetY = 0;
				if (drawInfo.drawPlayer.bodyFrame.Y >= 7 * drawInfo.drawPlayer.bodyFrame.Height &&
					drawInfo.drawPlayer.bodyFrame.Y <= 9 * drawInfo.drawPlayer.bodyFrame.Height ||
					drawInfo.drawPlayer.bodyFrame.Y >= 14 * drawInfo.drawPlayer.bodyFrame.Height &&
					drawInfo.drawPlayer.bodyFrame.Y <= 16 * drawInfo.drawPlayer.bodyFrame.Height) {
					offsetY = -2;
				}

				int bodyframe = drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height;
				Rectangle sourceRect = new(0, bodyframe * (18 + 38) + 33, texture.Width, 18);
				Vector2 origin = sourceRect.Size() / 2;
				float h_pi = MathHelper.Pi / 2;
				float r = h_pi - h_pi * drawInfo.drawPlayer.gravDir;

				drawInfo.DrawDataCache.Add(
					new DrawData(texture, new Vector2(drawX, drawY + offsetY + drawInfo.drawPlayer.gfxOffY),
						sourceRect, drawInfo.colorArmorBody, r, origin, 1f,
						drawInfo.drawPlayer.gravDir * drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0) {
						shader = dyeShader
					});
			}
		}
	}
}
