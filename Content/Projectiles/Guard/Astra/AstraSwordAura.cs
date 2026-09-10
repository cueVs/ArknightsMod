using ArknightsMod.Content.Items.Weapons.Guard.Astra;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Astra
{
	/// <summary>
	/// 星辉剑（二技能）开启时的背后光环：纯外观弹幕，64×64，贴图 AstraSwordAura。
	/// 每帧锁定在玩家身后的肩/躯干高度（面朝方向的反向、紧贴身体），并逐帧检查
	/// "技能仍在激活且仍手持武器"，任一条件不满足（技能结束 / 切换物品 / 死亡）即自行销毁。
	/// 弹幕默认绘制在玩家之前 → 光环中段会被玩家身体挡住，四周只露出光环边缘（贴背圣光效果）。
	/// 生成由武器端的 EnsureAura 负责（本地玩家），这里只负责跟随、发光与销毁。
	/// </summary>
	public class AstraSwordAura : ModProjectile
	{
		// 相对玩家 MountedCenter 的偏移：
		//  · X：紧贴身体（12px），绘制顺序在玩家之前，足以被玩家躯干挡住中段；
		//  · Y：抬升到躯干上段/肩颈后方（-22px），64px 光环下缘约收在躯干上部，
		//       不会垂到腿部。
		private const float OffsetX = 12f;
		private const float OffsetY = -22f;

		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = false;   // 纯外观，不参与任何伤害/碰撞
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.alpha = 60;         // 轻微半透明（配合 PostDraw 的加法发光）
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 2;       // 每帧由 AI 续命，条件失效则自然死亡
			Projectile.penetrate = -1;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner == null || !owner.active || owner.dead) {
				Projectile.Kill();
				return;
			}

			// "技能是否仍激活"由属主所在的客户端判定（WeaponPlayer 的技能状态主要在客户端
			// 模拟，服务器端拿到的值不可靠）。属主客户端判定失效并 Kill 后，销毁会经网络
			// 同步到服务器与其它端；其它端这里只负责跟随，不自行 Kill，避免误杀。
			if (Projectile.owner == Main.myPlayer) {
				var wp = owner.GetModPlayer<WeaponPlayer>();
				bool stillActive = owner.HeldItem?.ModItem is AstraSword
					&& wp.SkillActive && wp.Skill == 1;

				// 技能结束 / 切走武器 → 消失
				if (!stillActive) {
					Projectile.Kill();
					return;
				}
			}

			Projectile.timeLeft = 2; // 续命

			// 每帧同步到玩家身后（紧贴身体、抬至肩/躯干高度）
			Projectile.Center = owner.MountedCenter
				+ new Vector2(-owner.direction * OffsetX, OffsetY);

			// 动画计时与脉动
			Projectile.localAI[0]++;

			// 发光：为周围环境注入蓝色光
			float pulse = 1f + 0.18f * (float)Math.Sin(Projectile.localAI[0] * 0.08f);
			Lighting.AddLight(Projectile.Center,
				0.22f * pulse, 0.42f * pulse, 0.75f * pulse);
		}

		// 发光绘制：默认（AlphaBlend）绘制半透明本体之后，再用 Additive 叠加两层光晕，
		// 外圈淡蓝 + 内芯亮白，并带轻微呼吸脉动，形成自发光效果。
		public override void PostDraw(Color lightColor) {
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			if (tex == null || tex.IsDisposed)
				return;

			Vector2 pos = Projectile.Center - Main.screenPosition;
			Vector2 origin = tex.Size() / 2f;
			float t = Projectile.localAI[0];
			float pulse = 1f + 0.08f * (float)Math.Sin(t * 0.08f);
			float alpha = 0.55f;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// 外层：淡蓝辉光（略大、脉动）
			Main.spriteBatch.Draw(tex, pos, null,
				new Color(90, 170, 255) * alpha, Projectile.rotation,
				origin, Projectile.scale * 1.18f * pulse, SpriteEffects.None, 0f);
			// 内层：亮白核心（略小）
			Main.spriteBatch.Draw(tex, pos, null,
				new Color(235, 248, 255) * alpha * 0.55f, Projectile.rotation,
				origin, Projectile.scale * 0.9f * pulse, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}
