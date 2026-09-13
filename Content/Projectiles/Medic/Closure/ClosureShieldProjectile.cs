using System;
using System.Collections.Generic;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·一技能护盾可视化（纯代码绘制，无贴图）：
	//   在玩家本人、其它玩家、以及玩家自身的召唤物（援军）位置各套一个蓝色圆球护盾——
	//   球心可见（低不透明度），越靠近上下两极不透明度越高，边缘有一圈清晰的亮蓝描边，
	//   整体像一层包裹住目标的半透明气泡护盾。
	//   本弹幕只负责"画"，一技能开启期间存在、结束即销毁；不参与任何碰撞/伤害逻辑。
	public class ClosureShieldProjectile : ModProjectile
	{
		private static readonly Color BubbleBlue = new(110, 185, 255);
		private static readonly Color RimBlue    = new(200, 235, 255);

		private static BasicEffect _basic;

		private int _age;

		public override string Texture => ArknightsMod.noTexture;

		public override void Unload() {
			Main.QueueMainThreadAction(() => {
				_basic?.Dispose();
				_basic = null;
			});
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly    = false;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = -1;
			Projectile.netImportant = true;
			Projectile.timeLeft    = 99999; // 由一技能开启状态手动管理
		}

		/// <summary>若该玩家当前没有护盾可视化弹幕，则生成一个（避免重复）。</summary>
		public static void EnsureFor(Player player) {
			if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
				return;
			int type = ModContent.ProjectileType<ClosureShieldProjectile>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (p.active && p.type == type && p.owner == player.whoAmI)
					return;
			}
			Projectile.NewProjectile(player.GetSource_Misc("ClosureShield"), player.Center, Vector2.Zero,
				type, 0, 0f, player.whoAmI);
		}

		private bool OwnerShieldActive {
			get {
				Player owner = Main.player[Projectile.owner];
				if (owner == null || !owner.active || owner.dead)
					return false;
				var mp = owner.GetModPlayer<WeaponPlayer>();
				return mp.Skill == 0 && mp.SkillActive; // 一技能开启期间
			}
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
			_age++;

			Player owner = Main.player[Projectile.owner];
			if (owner != null && owner.active)
				Projectile.Center = owner.Center;

			if (!OwnerShieldActive) {
				Projectile.Kill();
				return;
			}
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		// 画一个蓝色气泡护盾：只在上下两极显现（中间部分和左右两侧完全透明），
		// 边缘描边也只在上下两极保留，左右两侧的描边淡化消失。
		private static void AddBubble(List<VertexPositionColor> verts, Vector2 pos, float radius, float alpha) {
			const int seg = 48;

			// 1. 填充：从球心（完全透明）向边缘渐变；只在接近上下两极时才逐渐显现（左右两侧完全透明）。
			Color center = BubbleBlue * 0f; // 中心完全透明
			for (int i = 0; i < seg; i++) {
				float a0 = MathHelper.TwoPi * i / seg;
				float a1 = MathHelper.TwoPi * (i + 1) / seg;
				// |sin| 接近 1 时（上下两极）才显现，接近 0 时（左右两侧）完全透明
				float pole0 = Math.Abs((float)Math.Sin(a0));
				float pole1 = Math.Abs((float)Math.Sin(a1));
				// 进一步提升对比度：小于 0.5 的区域直接压到 0
				pole0 = pole0 < 0.5f ? 0f : (pole0 - 0.5f) * 2f;
				pole1 = pole1 < 0.5f ? 0f : (pole1 - 0.5f) * 2f;
				Color e0 = BubbleBlue * (alpha * pole0 * 0.65f);
				Color e1 = BubbleBlue * (alpha * pole1 * 0.65f);
				AddTri(verts, pos,
					pos + a0.ToRotationVector2() * radius,
					pos + a1.ToRotationVector2() * radius, center, e0, e1);
			}

			// 2. 边缘描边：只在上下两极保留，左右两侧的描边完全透明。
			float rimInner = radius * 0.88f;
			float rimOuter = radius;
			for (int i = 0; i < seg; i++) {
				float a0 = MathHelper.TwoPi * i / seg;
				float a1 = MathHelper.TwoPi * (i + 1) / seg;
				float pole0 = Math.Abs((float)Math.Sin(a0));
				float pole1 = Math.Abs((float)Math.Sin(a1));
				// 只在两极附近显示描边
				pole0 = pole0 < 0.5f ? 0f : (pole0 - 0.5f) * 2f;
				pole1 = pole1 < 0.5f ? 0f : (pole1 - 0.5f) * 2f;
				Color r0 = RimBlue * (alpha * pole0 * 0.85f);
				Color r1 = RimBlue * (alpha * pole1 * 0.85f);
				Color clear0 = r0; clear0.A = 0;
				Color clear1 = r1; clear1.A = 0;
				Vector2 i0 = pos + a0.ToRotationVector2() * rimInner;
				Vector2 i1 = pos + a1.ToRotationVector2() * rimInner;
				Vector2 o0 = pos + a0.ToRotationVector2() * rimOuter;
				Vector2 o1 = pos + a1.ToRotationVector2() * rimOuter;
				AddTri(verts, i0, o0, o1, clear0, r0, r1);
				AddTri(verts, i0, o1, i1, clear0, r1, clear1);
			}
		}

		private static float RadiusFor(Entity e) {
			float r = Math.Max(e.width, e.height) * 0.75f + 10f;
			return MathHelper.Clamp(r, 22f, 90f);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			// 轻微呼吸脉动
			float pulse = 0.88f + 0.12f * (float)Math.Sin(_age * 0.08f);

			var verts = new List<VertexPositionColor>(512);

			// 玩家本人 + 其它玩家（援军）
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player p = Main.player[i];
				if (!p.active || p.dead)
					continue;
				// 只给护盾拥有者本人 + 其它玩家套盾（"援军 = 自身召唤物 + 其它玩家"）
				Vector2 sp = p.Center - Main.screenPosition;
				AddBubble(verts, sp, RadiusFor(p), pulse);
			}

			// 玩家自身的召唤物（minion / sentry）
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != Projectile.owner)
					continue;
				if (!(p.minion || p.sentry))
					continue;
				Vector2 sp = p.Center - Main.screenPosition;
				AddBubble(verts, sp, RadiusFor(p), pulse);
			}

			if (verts.Count < 3)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			_basic.Projection = Main.GameViewMatrix.TransformationMatrix * projection;

			var arr = verts.ToArray();
			foreach (EffectPass pass in _basic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
