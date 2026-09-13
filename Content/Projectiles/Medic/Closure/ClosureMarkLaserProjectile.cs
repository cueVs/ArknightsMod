using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·二技能激光：从标记中心射向目标的激光束（中心白色细线，两侧红色外发光柔和淡出）。
	// ai[0] = 目标 NPC 下标。持续攻击期间是同一个实例持续显示（不会每次攻击都重新生成/消失一次），
	// 每次 Volley 触发时只续命 + 重播一次"从标记中心射出、延伸到目标"的过渡动画；
	// 停火一段时间或目标失效后才淡出销毁。
	//
	// 绘制方式：用 BasicEffect + DrawUserPrimitives 手绘三角形（与本 mod 其它自定义特效一致），
	// 而不是 SpriteBatch.Draw(TextureAssets.MagicPixel...)——后者在当前 tModLoader 版本下，
	// MagicPixel 实际引用的贴图未必是单纯的 1x1 白像素（可能位于贴图图集中），直接拉伸绘制
	// 会整张图集贴图被拉伸，表现为"硬边缘 + 一侧无限延伸的白色矩形"的诡异效果。
	// 手绘三角形可以精确控制核心/发光各自的顶点颜色和透明度渐变，不依赖具体贴图内容。
	public class ClosureMarkLaserProjectile : ModProjectile
	{
		private const int KeepAliveTicks = 24; // 需大于武器二技能攻击间隔（14 tick），保证持续攻击时不闪烁
		private const int FadeOutTicks   = 10;
		private const int GrowTicks      = 6;  // 每次触发后，激光从标记中心"射出"到目标的过渡时长

		private const float CoreHalfWidth = 1.05f; // 中心白色核心的半宽
		private const float GlowHalfWidth = 11f;   // 红色外发光延伸到的半宽（核心之外的柔和淡出）
		private const int   CoreSegments  = 14;    // 白色核心沿长度方向的细分段数（用于渲染雾气过渡）

		private static readonly Color CoreCol = new(255, 255, 255);
		private static readonly Color GlowCol = new(235, 60, 70);
		private static readonly Color MistCol = new(235, 130, 130); // 浅红色半透明遮罩烟雾的色调

		private static BasicEffect _basic;

		private int _keepAlive = KeepAliveTicks;
		private bool _fadingOut;
		private int _fadeAge;
		private int _growAge; // 每次触发后清零，驱动"从标记射向目标"的伸长过渡动画
		private Vector2 _lastFromPos;

		// 白色核心上随机出现一段"蒙上浅红色雾气"的区域：中心位置（沿全长比例）+ 半长（比例）。
		// 每次触发都重新随机一次，让同一条持续显示的激光每次"射出"看起来不完全一样。
		private float _mistCenterT;
		private float _mistHalfLen;

		// 命中计数：每三次命中里，一次生成完整的取景框命中特效（复用普攻弹幕的命中特效），
		// 另外两次生成"次级"的红色不完整方框描边特效，二者都在目标附近随机偏移一点、
		// 命中后就不再跟随目标移动（区别于常驻的爆炸尖角特效，那个才需要贴着目标实时移动）。
		private int _hitCount;

		public override string Texture => ArknightsMod.noTexture;

		public override void Unload() {
			Main.QueueMainThreadAction(() => {
				_basic?.Dispose();
				_basic = null;
			});
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly    = true;
			Projectile.DamageType  = DamageClass.Ranged;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			// 持续存在（不再是每发独立生成、8 帧就消失），生命周期改由 _keepAlive/_fadingOut 手动管理。
			Projectile.penetrate   = -1;
			Projectile.timeLeft    = 99999;
			// 命中冷却须小于武器二技能攻击间隔（14 tick），保证同一个持续存在的实例每次 Volley
			// 脉冲都能重新命中一次，而不是全程只伤害一次。
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown  = 10;
		}

		public override void OnSpawn(IEntitySource source) {
			_keepAlive = KeepAliveTicks;
			_growAge = 0;
			_lastFromPos = Projectile.Center;
			RerollMist();
		}

		private void RerollMist() {
			_mistCenterT = Main.rand.NextFloat(0.15f, 0.85f);
			_mistHalfLen = Main.rand.NextFloat(0.18f, 0.30f);
		}

		/// <summary>标记每次 Volley 时调用：续命 + 更新伤害/目标 + 重播一次"射出"过渡动画。</summary>
		public void Refresh(int damage, float knockback, int targetIdx) {
			Projectile.damage = damage;
			Projectile.knockBack = knockback;
			Projectile.ai[0] = targetIdx;
			_keepAlive = KeepAliveTicks;
			_fadingOut = false;
			_fadeAge = 0;
			_growAge = 0;
			RerollMist();
		}

		// 找到本激光对应的目标标记（同一个 owner、锁定同一个目标）的当前中心，激光起点持续跟随它，
		// 而不是固定在生成时的某一个坐标（标记会跟随目标移动）。
		private Vector2 FindMarkerCenter(int targetIdx, Vector2 fallback) {
			for (int j = 0; j < Main.maxProjectiles; j++) {
				Projectile p = Main.projectile[j];
				if (p.active && p.owner == Projectile.owner
				    && p.ModProjectile is ClosureTargetMarkProjectile
				    && (int)p.ai[0] == targetIdx)
					return p.Center;
			}
			return fallback;
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;

			int idx = (int)Projectile.ai[0];
			if (idx < 0 || idx >= Main.maxNPCs || !Main.npc[idx].active) {
				Projectile.Kill();
				return;
			}

			NPC target = Main.npc[idx];
			Vector2 from = FindMarkerCenter(idx, _lastFromPos);
			_lastFromPos = from;
			Projectile.Center = (from + target.Center) * 0.5f;

			if (_growAge < GrowTicks)
				_growAge++;

			if (!_fadingOut) {
				if (_keepAlive > 0)
					_keepAlive--;
				else
					_fadingOut = true;
			}
			if (_fadingOut) {
				_fadeAge++;
				if (_fadeAge > FadeOutTicks)
					Projectile.Kill();
			}

			Lighting.AddLight(Projectile.Center, 0.5f, 0.15f, 0.25f);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			int idx = (int)Projectile.ai[0];
			if (idx < 0 || idx >= Main.maxNPCs)
				return false;
			// 只击中指定目标
			NPC target = Main.npc[idx];
			return target.active && target.Hitbox.Intersects(targetHitbox);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/ClosureScanGunShot") {
				Volume = 0.35f,
				Pitch = 0.3f,
			}, target.Center);

			// 三技能：施加可叠加的「迟钝」减速（6%/层，最高 60%，持续 3 秒）
			Content.Buffs.SluggishNPC.AddStack(target);

			if (Projectile.owner != Main.myPlayer)
				return;

			// 常驻的爆炸尖角命中特效：每次命中都生成，全程贴着目标当前位置移动
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
				ModContent.ProjectileType<ClosureLaserImpactProjectile>(), 0, 0, Projectile.owner,
				0f, target.whoAmI);

			// 每三次命中里穿插一次更醒目的取景框特效（复用普攻弹幕命中特效），
			// 另外两次显示次级的红色不完整方框描边——两者位置都在目标附近随机偏移，命中后不跟随目标。
			_hitCount++;
			Vector2 fxPos = target.Center + Main.rand.NextVector2Circular(14f, 14f);
			if (_hitCount % 3 == 0) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), fxPos, Vector2.Zero,
					ModContent.ProjectileType<ClosureHitBurstProjectile>(), 0, 0, Projectile.owner,
					Main.rand.Next(1, int.MaxValue));
			}
			else {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), fxPos, Vector2.Zero,
					ModContent.ProjectileType<ClosurePartialSquareProjectile>(), 0, 0, Projectile.owner);
			}
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		private static void AddQuad(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c, Vector2 d,
			Color ca, Color cb, Color cc, Color cd) {
			AddTri(v, a, b, c, ca, cb, cc);
			AddTri(v, a, c, d, ca, cc, cd);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			int idx = (int)Projectile.ai[0];
			if (idx < 0 || idx >= Main.maxNPCs || !Main.npc[idx].active)
				return false;

			NPC target = Main.npc[idx];
			Vector2 from = _lastFromPos - Main.screenPosition;
			Vector2 fullTo = target.Center - Main.screenPosition;

			float fadeOut = _fadingOut ? MathHelper.Clamp(1f - _fadeAge / (float)FadeOutTicks, 0f, 1f) : 1f;
			if (fadeOut <= 0.01f)
				return false;

			// "射出"过渡：激光末端从标记中心（0%）缓出延伸到目标（100%），而非瞬间就是全长。
			float growT = MathHelper.Clamp(_growAge / (float)GrowTicks, 0f, 1f);
			growT = 1f - (1f - growT) * (1f - growT); // 缓出
			Vector2 to = Vector2.Lerp(from, fullTo, growT);

			Vector2 dir = to - from;
			float len = dir.Length();
			if (len < 1f)
				return false;
			dir.Normalize();
			Vector2 perp = new(-dir.Y, dir.X);

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			var verts = new List<VertexPositionColor>(24 + CoreSegments * 6);

			Color coreOpaque = CoreCol * fadeOut;
			Color glowOpaque = GlowCol * (fadeOut * 0.75f);
			Color glowClear  = GlowCol * fadeOut; glowClear.A = 0;

			// 中心白色核心：沿长度方向细分，随机一段较长的区域蒙上浅红色半透明雾气
			// （颜色偏向 MistCol、透明度略降），两端用 smoothstep 柔和过渡而非硬边界。
			Vector2[] crossPos = new Vector2[CoreSegments + 1];
			Color[] crossCol = new Color[CoreSegments + 1];
			for (int i = 0; i <= CoreSegments; i++) {
				float ct = i / (float)CoreSegments;
				crossPos[i] = Vector2.Lerp(from, to, ct);

				float d = System.Math.Abs(ct - _mistCenterT);
				float w = 1f - MathHelper.Clamp(d / _mistHalfLen, 0f, 1f);
				w = w * w * (3f - 2f * w); // smoothstep：柔和边缘，不生硬

				Color tinted = Color.Lerp(coreOpaque, MistCol * fadeOut, w * 0.85f);
				tinted.A = (byte)(coreOpaque.A * MathHelper.Lerp(1f, 0.65f, w));
				crossCol[i] = tinted;
			}
			for (int i = 0; i < CoreSegments; i++) {
				Vector2 a0 = crossPos[i]     + perp * CoreHalfWidth;
				Vector2 a1 = crossPos[i]     - perp * CoreHalfWidth;
				Vector2 b1 = crossPos[i + 1] - perp * CoreHalfWidth;
				Vector2 b0 = crossPos[i + 1] + perp * CoreHalfWidth;
				AddQuad(verts, a0, a1, b1, b0, crossCol[i], crossCol[i], crossCol[i + 1], crossCol[i + 1]);
			}

			// 两侧红色外发光：从核心边缘（较浓）向外柔和淡出到透明
			Vector2 gTop0 = from + perp * CoreHalfWidth;
			Vector2 gTop1 = from + perp * GlowHalfWidth;
			Vector2 gTop2 = to   + perp * GlowHalfWidth;
			Vector2 gTop3 = to   + perp * CoreHalfWidth;
			AddQuad(verts, gTop0, gTop1, gTop2, gTop3, glowOpaque, glowClear, glowClear, glowOpaque);

			Vector2 gBot0 = from - perp * CoreHalfWidth;
			Vector2 gBot1 = from - perp * GlowHalfWidth;
			Vector2 gBot2 = to   - perp * GlowHalfWidth;
			Vector2 gBot3 = to   - perp * CoreHalfWidth;
			AddQuad(verts, gBot0, gBot1, gBot2, gBot3, glowOpaque, glowClear, glowClear, glowOpaque);

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			// 必须叠加 GameViewMatrix（游戏内缩放）再接正交投影，否则缩放不为默认值时会错位/不可见
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
