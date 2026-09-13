using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	// 弑君者刀光：纯视觉。ai[0]=斩/刺；ai[1]=spriteDirection；ai[2]=Boss whoAmI。
	public class SwordSlashEffect : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/Crownslayer_Slash";

		private const int FrameW = 134;
		private const int FrameH = 82;

		private static readonly int[] SlashRows = { 1, 2, 3, 4 };
		private static readonly int[] ThrustRows = { 6, 7, 8, 9 };

		private bool IsThrust => Projectile.ai[0] > 0.5f;
		private int FrameSpeed => IsThrust ? 7 : 4;

		private static Vector2 BladeOffsetFromBoss(int lockedSpriteDir) => new Vector2(lockedSpriteDir * -28f, -8f);

		public override void SetDefaults() {
			Projectile.width = FrameW;
			Projectile.height = FrameH;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 4 * 7 + 4;
			Projectile.alpha = 0;
			Projectile.scale = 1.85f;
		}

		public override bool? CanDamage() => false;

		public override bool ShouldUpdatePosition() => false;

		private int GetOwnerIndex() {
			int idx = (int)Projectile.ai[2];
			if (idx <= 0)
				idx = (int)Projectile.localAI[1];
			return idx;
		}

		public override void AI() {
			if (Projectile.localAI[0] == 0f)
				Projectile.timeLeft = 4 * FrameSpeed + 4;

			Projectile.velocity = Vector2.Zero;
			Projectile.localAI[0]++;

			int ownerIndex = GetOwnerIndex();
			if (ownerIndex > 0 && ownerIndex < Main.maxNPCs) {
				NPC owner = Main.npc[ownerIndex];
				if (owner.active && owner.type == ModContent.NPCType<Crownslayer>()) {
					int lockedDir = (int)Projectile.ai[1];
					if (lockedDir == 0)
						lockedDir = owner.spriteDirection;
					Projectile.Center = owner.Center + BladeOffsetFromBoss(lockedDir);
				}
				else {
					Projectile.Kill();
					return;
				}
			}

			if (Projectile.timeLeft <= 8)
				Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.timeLeft / 8f);
		}

		private Rectangle GetFrameRect() {
			int frameIdx = Math.Min((int)(Projectile.localAI[0] / FrameSpeed), 3);
			int row = IsThrust ? ThrustRows[frameIdx] : SlashRows[frameIdx];
			return new Rectangle(0, row * FrameH, FrameW, FrameH);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.alpha >= 254)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle src = GetFrameRect();
			Vector2 origin = src.Size() * 0.5f;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			float opacity = 1f - Projectile.alpha / 255f;

			// 贴图正方向为右；方向仅由生成时锁定的 rotation 决定，不再随 Boss 转向或翻转
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.Additive,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);

			Main.EntitySpriteDraw(tex, pos, src,
				new Color(255, 72, 24) * opacity * 0.55f,
				Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0);

			Main.EntitySpriteDraw(tex, pos, src,
				new Color(255, 220, 180) * opacity * 0.88f,
				Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			Main.EntitySpriteDraw(tex, pos, src,
				Color.White * opacity * 0.32f,
				Projectile.rotation, origin, Projectile.scale * 0.55f, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
