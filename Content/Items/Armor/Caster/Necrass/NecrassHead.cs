using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Necrass
{
	[AutoloadEquip(EquipType.Head)]
	public class NecrassHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		// 旧系统这里 IsArmorSet/UpdateArmorSet 只是把 player.setBonus 设成空字符串，
		// ArmorSets.hjson 里也从来没写过 Necrass 的 HelmetEffect/SetEffect/SetBonus——
		// 套装效果始终是空的。SetBonusKey 留 null 就是这个"没有文本"的等价物。
		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 166,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Necrass",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OrirockConcentration>(10)
				.AddIngredient<LoxicKohl>(10),
		};

		// 叠加在头部帧动画下层的补充图层，与 NecrassHead_Head 共用同样的 40x1120（20 帧
		// 竖排，每帧 40x56）结构，按当前动画帧号同步取帧绘制。
		internal class NecrassHeadBackHairLayer : PlayerDrawLayer
		{
			// 后发要画在**身体后面**，会被皮肤/衣服/头部挡住。
			// 之前挂在 BeforeParent(Head)（Head 是第 21 层）导致它盖在 Skin(12)、
			// Torso(17)、NeckAcc(20) 这些之上，看起来就是"糊住了整个角色"。
			// HeadBack 是原版专门画"后脑/脑后挂件"的层，位置在 BackAcc 之后、Skin 之前，
			// 正好符合"在身体之后被遮挡"的需求。
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeadBack);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 时装和套装都要认。判断走 IsPartVisible（看穿的是哪个物品），
				// 不比对 player.head 槽位 ID——原因见 IsPartVisible 的注释。
				return NeoArmorReforgeSetLoader.IsPartVisible<NecrassHead>(player, EquipType.Head) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Player p = drawInfo.drawPlayer;

				Texture2D texture = ModContent.Request<Texture2D>
					("ArknightsMod/Content/Items/Armor/Caster/Necrass/NecrassHead_BackHair").Value;

				// ⚠ 取帧必须用 bodyFrame，**不能用 headFrame**。
				// Player.headFrame 在整个 tModLoader 里只有一个写入者
				// （UICharacter.UpdateAnim，即角色选择界面的预览动画），游戏内实际
				// 行走/战斗时从来不赋值，恒为 {0,0,0,0}。用它取帧的结果就是
				// Width/Height 为 0、被判为非法直接 return，图层永远画不出来且无报错。
				// 原版画头部装备的 DrawPlayer_21_Head 用的也是 bodyFrame——头部帧表和
				// 躯干帧表本来就是同一套 20 帧 x 56px 的结构。
				Rectangle frame = p.bodyFrame;
				if (frame.Height <= 0)
					return;

				int frameIndex = frame.Y / frame.Height;
				Rectangle sourceRect = new(0, frameIndex * frame.Height, texture.Width, frame.Height);
				if (sourceRect.Bottom > texture.Height)
					return;

				// 坐标必须完整照抄原版画头部装备（DrawPlayer_21_Head）的公式，缺任何一
				// 项都会偏。反编译出来的三步是：
				//   1) 先算 (-bodyFrame.Width/2 + width/2, height - bodyFrame.Height + 4)
				//      —— 这两项就是"从碰撞箱左上角挪到头部帧左上角"的修正，之前少了它
				//      们，所以整体偏到了右下方约 (10, 10) 像素；
				//   2) 加上 Position - screenPosition 之后**整体 Utils.Floor 取整**
				//      —— headVect 的 Y 是 22.4 这种小数，不取整就会半像素采样，
				//      表现就是"游戏里的像素和贴图像素对不上"（发虚/错行）；
				//   3) 最后才加 headPosition + headVect，并且 origin 用 headVect
				//      （这样 headRotation 的旋转轴心和原版头部一致）。
				Vector2 position = Utils.Floor(
					new Vector2(
						-(p.bodyFrame.Width / 2) + (p.width / 2),
						p.height - p.bodyFrame.Height + 4f)
					+ drawInfo.Position - Main.screenPosition)
					+ p.headPosition + drawInfo.headVect;

				Vector2 origin = drawInfo.headVect;

				SpriteEffects effects = p.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				if (p.gravDir == -1)
					effects |= SpriteEffects.FlipVertically;

				drawInfo.DrawDataCache.Add(
					new DrawData(texture, position, sourceRect, drawInfo.colorArmorHead, p.headRotation, origin, 1f, effects, 0) {
						shader = drawInfo.cHead
					});
			}
		}
	}
}
