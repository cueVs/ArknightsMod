using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Assets.Effects;

namespace ArknightsMod.Content.Projectiles.Guard.Lupine
{
	// ============================================================
	//  狼之绯 挥砍弹幕
	//  刀光 = 由刀尖拖曳出的紫红色主体带（Flow pass：噪声调制颜色/明暗 + 沿挥砍反方向
	//         流动 + 尾部水墨式溶解消散）
	//       + 刀尖轨迹单独描一条高亮白线
	//       + 背景屏幕扭曲（安全抓屏模式，扭曲偏移取自与可见刀光同一张噪声贴图，
	//         使扭曲的形状天然贴合可见的颜色纹理）
	//  —— 手持贴图 LupineScarlet_protile（横向，尖右柄左）
	//
	//  参考 tEffectSkill 的 slash-mechanics.md（多段连击状态机）与
	//  slash-texture.md（真实屏幕扭曲：偏移场复用可见纹理的技巧）；
	//  水墨溶解与"随机强度纹理"的具体做法是本次新写的，不是照抄某个已有实现。
	// ============================================================
	public class LupineScarletSwingProjectile : ModProjectile
	{
		private const string RibbonShapePath = "ArknightsMod/Content/Projectiles/Guard/Lupine/LupineRibbon";
		private const string BladeTexPath    = "ArknightsMod/Content/Items/Weapons/Guard/Lupine/LupineScarlet_protile";
		private const string NoiseTexPath    = "ArknightsMod/Content/Projectiles/Rogue/FireworksHand/NoiseTexture";
		private const string DistortShaderPath = "ArknightsMod/Assets/Effects/LupineSlashDistortion";

		// === 可调常量（视觉微调入口）===
		private const float RibbonWidthMul   = 1.0f;
		private const float FlowScrollSpeed  = 0.9f;   // 噪声滚动速度（沿挥砍反方向）
		private const float DissolveAmount   = 0.85f;  // 尾部水墨溶解强度，0=不溶解
		private const float AccentThreshold  = 0.72f;  // 噪声亮度超过此值处混入更亮高光
		private const float NoiseScaleX      = 3.2f;   // 噪声沿长度方向的平铺密度
		private const float NoiseScaleY      = 1.4f;   // 噪声沿宽度方向的平铺密度
		private const float TipHighlightWidthMul = 0.14f; // 刀尖高亮线相对主体宽度的比例
		private const float DistortIntensity = 6.5f;   // 屏幕扭曲强度（像素）
		private const float DistortHalfWidth = 34f;    // 扭曲影响的半宽（像素，屏幕空间）

		private const int TrailLength = 24;
		private const float DisFromPlayer = 6f;

		// 挥砍状态
		private Vector2 mainVec;
		private Vector2[] trailVec;
		private int Timer;
		private int attackType;
		private const int maxAttackType = 2;
		private bool isAttacking;
		private bool oldIsAttacking;
		private bool UseTrail = true;
		private int killTimer;
		private bool hitThisAttack;
		private float lockedDir = 1f;

		// 当前段攻击的活跃窗口
		private int segActiveStart;
		private int segActiveEnd;

		// 水墨流动状态：滚动偏移 + 每段随机种子（每次进入新连击段时重新随机，
		// 让三段连击的纹理观感不完全一样——呼应 skill 里"每次挥砍不完全相同"的设计习惯）
		private Vector2 flowOffset;
		private Vector2 noiseSeedOffset;
		private float prevRotation;
		private bool hasPrevRotation;

		private static Asset<Effect> _distortEffect;
		private static Texture2D _mainColorTex;
		private static Texture2D _tipColorTex;

		Player Owner => Main.player[Projectile.owner];
		public override string Texture => BladeTexPath;

		public override void Load() {
			if (!Main.dedServ) {
				try {
					_distortEffect = ModContent.Request<Effect>(DistortShaderPath, AssetRequestMode.ImmediateLoad);
				} catch { _distortEffect = null; }
			}
		}

		public override void Unload() {
			_distortEffect = null;
			_mainColorTex = null;
			_tipColorTex = null;
		}

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 15;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 30;
			Projectile.scale = 1.15f;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
			Projectile.DamageType = DamageClass.Melee;
			trailVec = new Vector2[TrailLength];
		}

		public override bool ShouldUpdatePosition() => false;

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(attackType);
			writer.Write(lockedDir);
			writer.WriteVector2(mainVec);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			attackType = reader.ReadInt32();
			lockedDir = reader.ReadSingle();
			mainVec = reader.ReadVector2();
		}

		// ── AI ────────────────────────────────────────────
		public override void AI() {
			Player p = Owner;
			if (!p.active || p.dead || p.noItems || p.CCed) { Projectile.Kill(); return; }

			p.heldProj = Projectile.whoAmI;
			Projectile.Center = p.MountedCenter + Normalize(mainVec) * DisFromPlayer;
			Projectile.timeLeft = 30;
			isAttacking = false;

			p.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
				mainVec.ToRotation() - MathHelper.PiOver2);

			Attack(p);
			Timer++;

			// 水墨流动：沿"挥砍反方向"滚动——用旋转角的帧间变化量的符号反过来推流动偏移，
			// 这样即使连击段之间挥砍方向左右交替，流动方向也会自动跟着反转，而不是固定一个方向
			if (hasPrevRotation) {
				float delta = MathHelper.WrapAngle(Projectile.rotation - prevRotation);
				float dirSign = Math.Abs(delta) > 0.0001f ? Math.Sign(delta) : 0f;
				flowOffset.X -= dirSign * FlowScrollSpeed * 0.016f;
				flowOffset.Y += 0.05f * 0.016f; // 轻微的纵向漂移，避免看起来完全是一维滚动
			}
			prevRotation = Projectile.rotation;
			hasPrevRotation = true;

			bool shouldEnd = !isAttacking && !p.controlUseItem;
			if (shouldEnd) {
				killTimer++;
				if (killTimer > 8) { Projectile.Kill(); return; }
			} else {
				killTimer = 0;
			}

			if (isAttacking) {
				p.direction = (int)lockedDir;
				Projectile.spriteDirection = (int)lockedDir;
			}
			if (!oldIsAttacking && isAttacking) hitThisAttack = false;
			oldIsAttacking = isAttacking;

			if (UseTrail) {
				for (int i = TrailLength - 1; i > 0; i--) trailVec[i] = trailVec[i - 1];
				trailVec[0] = mainVec;
			} else {
				Array.Clear(trailVec, 0, TrailLength);
			}
		}

		private void LockDirFromMouse(Player p) {
			if (Main.myPlayer != Projectile.owner) return;
			float newDir = Main.MouseWorld.X > p.Center.X ? 1f : -1f;
			if (newDir != lockedDir) { lockedDir = newDir; Projectile.netUpdate = true; }
		}

		private void NextAttackType() {
			Timer = 0;
			attackType++;
			if (attackType > maxAttackType) attackType = 0;
			// 每进入新的一段连击就重新随机噪声采样起点，三段连击的纹理不会长得一模一样
			noiseSeedOffset = new Vector2(Main.rand.NextFloat(0f, 100f), Main.rand.NextFloat(0f, 100f));
		}

		// ── 三段连击 ──────────────────────────────────────
		private void Attack(Player p) {
			UseTrail = true;
			float dir = lockedDir;

			if (attackType == 0) {
				segActiveStart = 14; segActiveEnd = 34;
				if (Timer == 14) AttSound(SoundID.Item1);
				if (Timer < 14) {
					UseTrail = false; LockDirFromMouse(p); dir = lockedDir;
					float target = -MathHelper.PiOver2 - dir * 0.7f;
					mainVec = Vector2.Lerp(mainVec, target.ToRotationVector2() * 105f, 0.18f);
					mainVec += Normalize(mainVec) * 3f;
					Projectile.rotation = mainVec.ToRotation();
				}
				if (Timer > 14 && Timer < 34) {
					isAttacking = true;
					Projectile.rotation += dir * 0.13f;
					mainVec = Projectile.rotation.ToRotationVector2() * 108f;
					SpawnSwingDust(p);
				}
				if (Timer > 34) NextAttackType();
			}

			if (attackType == 1) {
				segActiveStart = 10; segActiveEnd = 30;
				if (Timer == 10) AttSound(SoundID.Item1);
				if (Timer < 10) {
					UseTrail = false; LockDirFromMouse(p); dir = lockedDir;
					float target = MathHelper.PiOver2 + dir * 0.9f;
					mainVec = Vector2.Lerp(mainVec, target.ToRotationVector2() * 108f, 0.22f);
					mainVec += Normalize(mainVec) * 3f;
					Projectile.rotation = mainVec.ToRotation();
				}
				if (Timer > 10 && Timer < 30) {
					isAttacking = true;
					Projectile.rotation -= dir * 0.14f;
					mainVec = Projectile.rotation.ToRotationVector2() * 112f;
					SpawnSwingDust(p);
				}
				if (Timer > 30) NextAttackType();
			}

			if (attackType == 2) {
				segActiveStart = 18; segActiveEnd = 42;
				if (Timer == 18) AttSound(SoundID.Item1);
				if (Timer < 18) {
					UseTrail = false; LockDirFromMouse(p); dir = lockedDir;
					float target = -MathHelper.PiOver2 * 1.1f - dir * 0.5f;
					mainVec = Vector2.Lerp(mainVec, target.ToRotationVector2() * 112f, 0.12f);
					mainVec += Normalize(mainVec) * 3f;
					Projectile.rotation = mainVec.ToRotation();
				}
				if (Timer > 18 && Timer < 42) {
					isAttacking = true;
					Projectile.rotation += dir * 0.10f;
					mainVec = Projectile.rotation.ToRotationVector2() * 118f;
					SpawnSwingDust(p);
				}
				if (Timer > 42) { attackType = -1; NextAttackType(); }
			}
		}

		private void AttSound(SoundStyle s) => SoundEngine.PlaySound(s, Projectile.Center);

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!isAttacking) return false;
			float point = 0f;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
					Projectile.Center + mainVec * Projectile.scale * 0.1f,
					Projectile.Center + mainVec * Projectile.scale,
					Projectile.height * 1.5f, ref point))
				return true;
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			hitThisAttack = true;
			modifiers.HitDirectionOverride = target.Center.X > Owner.Center.X ? 1 : -1;
		}

		public override void CutTiles() {
			if (!isAttacking) return;
			DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;
			Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + mainVec,
				Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
		}

		// ── 挥砍尘埃（紫红气焰，呼应刀光主色）──────────────
		private void SpawnSwingDust(Player p) {
			if (!Main.rand.NextBool(2)) return;
			for (int i = 3; i < 6; i++) {
				if (!Main.rand.NextBool()) continue;
				float frac = i / 5f;
				Vector2 spawnPos = Projectile.Center + mainVec * frac;
				bool bright = Main.rand.NextBool(3);
				int dustType = bright ? DustID.PinkFairy : DustID.PurpleTorch;
				Color dustColor = bright ? new Color(230, 170, 255) : new Color(150, 40, 130);
				Dust d = Dust.NewDustDirect(spawnPos, 6, 6, dustType, 0f, 0f, 0, dustColor,
					Main.rand.NextFloat(0.9f, 1.4f));
				d.noGravity = true;
				d.velocity = (mainVec.ToRotation() + lockedDir * MathHelper.PiOver2).ToRotationVector2()
					* Main.rand.NextFloat(1.0f, 2.6f);
				d.velocity += Main.rand.NextVector2Circular(0.7f, 0.7f);
			}
		}

		// ── 绘制 ──────────────────────────────────────────
		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ) return false;
			DrawScreenDistortion(); // 最底层：先扭曲背景，再在上面画刀光本体
			DrawSlashBody();        // 主体：紫红渐变 + 流动噪声 + 尾部水墨溶解
			DrawTipHighlight();     // 刀尖轨迹单独描白
			DrawBlade();            // 刀身
			return false;
		}

		private float TrailAlpha(float factor) {
			float smooth = MathHelper.SmoothStep(0f, 1f, factor);
			return MathHelper.Lerp(0.02f, 1.1f, smooth);
		}

		// 收集当前有效的拖尾点数，返回 (顶点用的 inner/outer 位置表, 有效计数)
		private int ValidTrailCount() {
			int counts = 0;
			for (int i = 0; i < trailVec.Length; i++) if (trailVec[i] != Vector2.Zero) counts++;
			return counts;
		}

		// 主体刀光：紫红渐变 + 噪声调制的流动纹理 + 尾部水墨式溶解（Flow pass）
		private void DrawSlashBody() {
			if (ArknightsMod.LupineKnifeLight?.Value == null) return;
			Texture2D colorTex = GetMainColorTex();
			Texture2D noiseTex = GetNoiseTex();
			if (colorTex == null || noiseTex == null) return;

			int counts = ValidTrailCount();
			if (counts < 2) return;

			var bars = new List<Vertex>();
			for (int j = 0; j < trailVec.Length; j++) {
				if (trailVec[j] == Vector2.Zero) continue;
				float factor = 1f - j / (float)counts;   // 1=刀尖当前位置(新)，→0=尾部(旧)
				float w = TrailAlpha(factor) * RibbonWidthMul;
				// inner 更靠近握持位置（刀柄一侧），outer 是刀尖拖曳出的外缘——
				// coord.y 在 0(inner)→1(outer) 间由光栅化插值，对应"越靠近刀柄颜色越深"的取值轴
				Vector2 inner = Projectile.Center + trailVec[j] * 0.12f * Projectile.scale;
				Vector2 outer = Projectile.Center + trailVec[j] * Projectile.scale;
				bars.Add(new Vertex(inner, new Vector3(factor, 0f, 0f), Color.White));
				bars.Add(new Vertex(outer, new Vector3(factor, 1f, w), Color.White));
			}
			if (bars.Count < 3) return;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);

			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f))
				* Main.GameViewMatrix.ZoomMatrix;
			Effect fx = ArknightsMod.LupineKnifeLight.Value;
			fx.Parameters["uTransform"].SetValue(model * projection);
			fx.Parameters["tex0"].SetValue(ModContent.Request<Texture2D>(RibbonShapePath).Value);
			fx.Parameters["tex1"].SetValue(colorTex);
			fx.Parameters["tex2"]?.SetValue(noiseTex);
			fx.Parameters["uFlowOffset"]?.SetValue(flowOffset + noiseSeedOffset);
			fx.Parameters["uNoiseScale"]?.SetValue(new Vector2(NoiseScaleX, NoiseScaleY));
			fx.Parameters["uDissolveAmount"]?.SetValue(DissolveAmount);
			fx.Parameters["uAccentThreshold"]?.SetValue(AccentThreshold);
			fx.Parameters["uAccentColor"]?.SetValue(new Vector3(0.85f, 0.55f, 1f)); // 更亮的紫、趋近白
			fx.CurrentTechnique.Passes["Flow"].Apply();

			Main.graphics.GraphicsDevice.DrawUserPrimitives(
				PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 刀尖轨迹单独描白：只取 outer（刀尖历史位置）连成一条细高亮线，不经过 Flow 噪声调制，
		// 保持纯净、不被溶解影响，和主体的"越靠近刀柄越深"形成对比
		private void DrawTipHighlight() {
			int counts = ValidTrailCount();
			if (counts < 2) return;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			// 注意：不能用 ArknightsMod/Assets/null——那是给"不显示任何东西"用的全透明像素(A=0)，
			// 这里要画实际可见的线，必须用原版 MagicPixel（不透明白色 1x1，专门给这种用途用）
			Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			float baseWidth = 30f * TipHighlightWidthMul * Projectile.scale;

			for (int j = 0; j < trailVec.Length - 1; j++) {
				if (trailVec[j] == Vector2.Zero || trailVec[j + 1] == Vector2.Zero) continue;
				float factor = 1f - j / (float)counts;
				float alpha = MathHelper.Clamp(factor * factor, 0f, 1f); // 比主体衰减更快，只在靠近刀尖处明显
				if (alpha <= 0.02f) continue;

				Vector2 a = Projectile.Center + trailVec[j] * Projectile.scale - Main.screenPosition;
				Vector2 b = Projectile.Center + trailVec[j + 1] * Projectile.scale - Main.screenPosition;
				Vector2 diff = b - a;
				float length = diff.Length();
				if (length < 0.5f) continue;
				float rot = diff.ToRotation();

				Main.spriteBatch.Draw(pixel, a, null, new Color(255, 250, 255) * alpha, rot,
					Vector2.Zero, new Vector2(length, baseWidth * alpha), SpriteEffects.None, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 背景屏幕扭曲——安全抓屏模式（参照 SchwarzArrow.DrawDistortionTrail），偏移场取自
		// 与可见刀光相同的噪声贴图 + 相同的 flowOffset，让扭曲的形状贴合可见的颜色纹理
		private void DrawScreenDistortion() {
			if (_distortEffect?.Value == null || !isAttacking) return;
			int counts = ValidTrailCount();
			if (counts < 2) return;

			var gd = Main.instance?.GraphicsDevice;
			if (gd == null) return;
			var screenRT = Main.screenTarget;
			if (screenRT == null || screenRT.IsDisposed) return;

			int sw = screenRT.Width, sh = screenRT.Height;

			if (LupineDistortionSystem.ScreenSnapshot == null ||
				LupineDistortionSystem.ScreenSnapshot.IsDisposed ||
				LupineDistortionSystem.ScreenSnapshot.Width  != sw ||
				LupineDistortionSystem.ScreenSnapshot.Height != sh) {
				LupineDistortionSystem.ScreenSnapshot?.Dispose();
				LupineDistortionSystem.ScreenSnapshot = new RenderTarget2D(gd, sw, sh, false, screenRT.Format, DepthFormat.None);
			}
			if (LupineDistortionSystem.CaptureBatch == null || LupineDistortionSystem.CaptureBatch.IsDisposed) {
				LupineDistortionSystem.CaptureBatch = new SpriteBatch(gd);
			}

			var snap = LupineDistortionSystem.ScreenSnapshot;
			var cap = LupineDistortionSystem.CaptureBatch;
			var eff = _distortEffect.Value;
			Texture2D noiseTex = GetNoiseTex();
			if (noiseTex == null) return;

			try { Main.spriteBatch.End(); } catch { }

			// 步骤1：把当前 screenTarget 内容抓到快照
			gd.SetRenderTarget(snap);
			cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			cap.Draw(screenRT, Vector2.Zero, Color.White);
			cap.End();

			// 步骤2：重新绑定 screenTarget 并整面填回（重绑会触发内容丢弃，必须先整面复原）
			gd.SetRenderTarget(screenRT);
			cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			cap.Draw(snap, Vector2.Zero, Color.White);
			cap.End();

			var snapSz = new Vector2(sw, sh);
			Matrix viewMtx = Main.GameViewMatrix.TransformationMatrix;
			float halfWidthUV = DistortHalfWidth / sw;
			float padPx = DistortIntensity + 8f;

			eff.Parameters["uScreenResolution"]?.SetValue(snapSz);
			eff.Parameters["tex1"]?.SetValue(noiseTex);
			eff.Parameters["uFlowOffset"]?.SetValue(flowOffset + noiseSeedOffset);
			eff.Parameters["uNoiseScale"]?.SetValue(new Vector2(NoiseScaleX, NoiseScaleY));
			eff.Parameters["uIntensity"]?.SetValue(DistortIntensity);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, eff, Matrix.Identity);

			for (int j = 0; j < trailVec.Length - 1; j++) {
				if (trailVec[j] == Vector2.Zero || trailVec[j + 1] == Vector2.Zero) continue;

				Vector2 posA = Projectile.Center + trailVec[j] * Projectile.scale;
				Vector2 posB = Projectile.Center + trailVec[j + 1] * Projectile.scale;
				Vector2 scrA = Vector2.Transform(posA - Main.screenPosition, viewMtx);
				Vector2 scrB = Vector2.Transform(posB - Main.screenPosition, viewMtx);

				Vector2 dirPx = scrB - scrA;
				float dirLen = dirPx.Length();
				if (dirLen < 0.5f) continue;
				Vector2 perpPx = new Vector2(-dirPx.Y * DistortHalfWidth / dirLen, dirPx.X * DistortHalfWidth / dirLen);

				float bminX = Math.Max(0f, MathF.Min(MathF.Min(scrA.X + perpPx.X, scrA.X - perpPx.X), MathF.Min(scrB.X + perpPx.X, scrB.X - perpPx.X)) - padPx);
				float bminY = Math.Max(0f, MathF.Min(MathF.Min(scrA.Y + perpPx.Y, scrA.Y - perpPx.Y), MathF.Min(scrB.Y + perpPx.Y, scrB.Y - perpPx.Y)) - padPx);
				float bmaxX = Math.Min(sw, MathF.Max(MathF.Max(scrA.X + perpPx.X, scrA.X - perpPx.X), MathF.Max(scrB.X + perpPx.X, scrB.X - perpPx.X)) + padPx);
				float bmaxY = Math.Min(sh, MathF.Max(MathF.Max(scrA.Y + perpPx.Y, scrA.Y - perpPx.Y), MathF.Max(scrB.Y + perpPx.Y, scrB.Y - perpPx.Y)) + padPx);
				if (bmaxX <= bminX || bmaxY <= bminY) continue;

				var srcRect = new Rectangle((int)bminX, (int)bminY, (int)(bmaxX - bminX), (int)(bmaxY - bminY));

				Vector2 uvA = scrA / snapSz;
				Vector2 uvB = scrB / snapSz;
				Vector2 uvDir = uvB - uvA;

				eff.Parameters["uTargetPosition"]?.SetValue(uvA);
				eff.Parameters["uDirection"]?.SetValue(uvDir);
				eff.Parameters["uImageSize1"]?.SetValue(new Vector2(uvDir.Length(), halfWidthUV));

				eff.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(snap, new Vector2(bminX, bminY), srcRect, Color.White);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 刀身：横向 protile（尖右柄左），柄锚定在玩家手部
		private void DrawBlade() {
			Texture2D tex = ModContent.Request<Texture2D>(BladeTexPath).Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.ZoomMatrix);

			bool faceLeft = lockedDir < 0;
			SpriteEffects fx = faceLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;
			float rot = mainVec.ToRotation();
			Vector2 origin = new Vector2(0f, tex.Height / 2f); // 柄在左中
			Color col = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
			float lenScale = mainVec.Length() / tex.Width; // 刀长贴合攻击距离

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, col, rot,
				origin, new Vector2(lenScale, 1f) * Projectile.scale, fx, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		// ── 程序生成主刀光渐变（紫红为主体，越靠近刀柄一侧越深）──
		// 采样轴对应 Flow pass 里的 coord.y（径向：0=inner/刀柄端 → 1=outer/刀尖端），
		// 不是挥砍历史轴 coord.x（那条轴管新旧/溶解，两件事分开控制）。
		// 注意：这里的"深"指亮度更低更暗，不是更透明——不透明度由 shape/dissolve 单独控制。
		private static Texture2D GetMainColorTex() {
			if (_mainColorTex != null) return _mainColorTex;
			if (Main.dedServ || Main.instance?.GraphicsDevice == null) return null;
			const int w = 256;
			var tex = new Texture2D(Main.instance.GraphicsDevice, w, 1);
			var data = new Color[w];
			for (int x = 0; x < w; x++) {
				float t = x / (w - 1f); // 0=刀柄端(深) → 1=刀尖端(亮)
				Color c;
				if (t < 0.5f) c = Color.Lerp(new Color(35, 5, 45), new Color(140, 30, 130), t / 0.5f);      // 深紫→紫红
				else if (t < 0.85f) c = Color.Lerp(new Color(140, 30, 130), new Color(200, 70, 200), (t - 0.5f) / 0.35f); // 紫红→亮紫红
				else c = Color.Lerp(new Color(200, 70, 200), new Color(255, 210, 255), (t - 0.85f) / 0.15f);  // 亮紫红→近白（刀尖最亮）
				data[x] = new Color(c.R, c.G, c.B, (byte)255);
			}
			tex.SetData(data);
			_mainColorTex = tex;
			return tex;
		}

		private static Texture2D GetNoiseTex() {
			try { return ModContent.Request<Texture2D>(NoiseTexPath, AssetRequestMode.ImmediateLoad).Value; }
			catch { return null; }
		}

		private static Vector2 Normalize(Vector2 v) => v == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(v);

		private struct Vertex : IVertexType {
			private static readonly VertexDeclaration _decl = new VertexDeclaration(
				new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
				new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));
			public Vector2 Position;
			public Color Color;
			public Vector3 TexCoord;
			public Vertex(Vector2 position, Vector3 texCoord, Color color) {
				Position = position; TexCoord = texCoord; Color = color;
			}
			public VertexDeclaration VertexDeclaration => _decl;
		}
	}
}
