using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Astesia
{
	/// <summary>
	/// 星辉剑·弧光：移植自 ASTES1A（星极）的 TaurusProj —— 原版 Excalibur 挥砍能量弧，
	/// 配色 #0080FF 蓝色。视觉与 Taurus 完全一致（四层剑光 + 细高光线 + 星芒 + 蓝色粒子）。
	/// 剑身本身不造成伤害，所有伤害由本弹幕的扇形碰撞承担。
	/// </summary>
	public class AstraSwordSlash : ModProjectile
	{
		// 复用原版 Excalibur 弹幕贴图（4 帧竖排），跨 mod 可用，无需自备贴图
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Excalibur;

		public override void SetStaticDefaults()
		{
			// 攻击正在放电的水母会被反伤（与 Night's Edge / Excalibur / Terra Blade 近战段同集合）
			ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
			Main.projFrames[Type] = 4; // 弹幕贴图共 4 帧（竖排）
		}

		public override void SetDefaults()
		{
			// 宽高不重要，因为使用了自定义碰撞
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 3;               // 最多命中 3 个敌人
			Projectile.usesLocalNPCImmunity = true; // 每个 NPC 独立记录免疫
			Projectile.localNPCHitCooldown = -1;    // 同一个 NPC 只被命中一次
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;        // 视线检查：不能隔墙命中
			Projectile.ownerHitCheckDistance = 300f; // 最大射程 300 像素 = 18.75 格
			Projectile.usesOwnerMeleeHitCD = true;  // 应用标准近战无敌帧
			// 打满穿透次数后停止伤害但继续存活，保证挥砍视觉完整
			Projectile.stopsDealingDamageAfterPenetrateHits = true;

			// 使用自定义 AI（原版 Excalibur 使用 aiStyle 190）
			Projectile.aiStyle = -1;

			// 自定义 AI 时关闭默认的药瓶视觉，改由 AI 沿弧线手动生成
			Projectile.noEnchantmentVisuals = true;
		}

		public override void AI()
		{
			// 参数协议（由武器 Shoot 传入）：
			// ai[0] == 方向（含重力翻转）
			// ai[1] == 最大存活时间（= 挥砍动画总长）
			// ai[2] == 物品缩放
			// localAI[0] == 已存活时间

			Projectile.localAI[0]++;
			Player player = Main.player[Projectile.owner];
			float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1]; // 生命进度 0→1
			float direction = Projectile.ai[0];
			float velocityRotation = Projectile.velocity.ToRotation();
			float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
			Projectile.rotation = adjustedRotation; // 在挥砍动画期间匀速扫过 180°

			float scaleMulti = 0.6f; // Excalibur/Terra Blade 为 0.6；True Excalibur 为 1
			float scaleAdder = 1f;

			Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
			Projectile.scale = scaleAdder + percentageOfLife * scaleMulti;

			// 沿弧线撒出粒子（#0080FF 蓝色）
			float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f;
			Vector2 dustPosition = Projectile.Center + dustRotation.ToRotationVector2() * 84f * Projectile.scale;
			Vector2 dustVelocity = (dustRotation + Projectile.ai[0] * MathHelper.PiOver2).ToRotationVector2();
			if (Main.rand.NextFloat() * 2f < Projectile.Opacity) {
				Color dustColor = Color.Lerp(new Color(0, 128, 255), Color.White, Main.rand.NextFloat() * 0.3f);
				Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), DustID.FireworksRGB, dustVelocity * 1f, 100, dustColor, 0.4f);
				coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
				coloredDust.noGravity = true;
			}

			if (Main.rand.NextFloat() * 1.5f < Projectile.Opacity) {
				Dust.NewDustPerfect(dustPosition, DustID.TintableDustLighted, dustVelocity, 100, new Color(0, 128, 255) * Projectile.Opacity, 1.2f * Projectile.Opacity);
			}

			Projectile.scale *= Projectile.ai[2]; // 乘上物品缩放

			// 生存期满后销毁
			if (Projectile.localAI[0] >= Projectile.ai[1]) {
				Projectile.Kill();
			}

			// 沿弧线生成药瓶（武器附着）视觉效果
			for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2) {
				Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 70f * Projectile.scale, new Vector2(60f * Projectile.scale, 60f * Projectile.scale));
				Projectile.EmitEnchantmentVisualsAt(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
			}
		}

		// 自定义碰撞：两个圆锥（扇形）覆盖挥砍扫过的整个弧区
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float coneLength = 94f * Projectile.scale; // 圆锥半径（94 与贴图尺寸匹配）
			float collisionRotation = MathHelper.Pi * 2f / 25f * Projectile.ai[0]; // 微调角度对齐贴图
			float maximumAngle = MathHelper.PiOver4; // 45° 半角，制造死区
			float coneRotation = Projectile.rotation + collisionRotation;

			// 第一个圆锥：当前帧角度处的扇形
			if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle)) {
				return true;
			}

			// 第二个圆锥：覆盖刚挥过的背面弧区（生命进度的 30%–50% 期间）
			float backOfTheSwing = Utils.Remap(Projectile.localAI[0], Projectile.ai[1] * 0.3f, Projectile.ai[1] * 0.5f, 1f, 0f);
			if (backOfTheSwing > 0f) {
				float coneRotation2 = coneRotation - MathHelper.PiOver4 * Projectile.ai[0] * backOfTheSwing;
				if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation2, maximumAngle)) {
					return true;
				}
			}

			return false;
		}

		// 挥砍可割草、打碎罐子、唤醒蜂后幼虫
		public override void CutTiles()
		{
			Vector2 starting = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
			Vector2 ending = (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
			float width = 60f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center + starting, Projectile.Center + ending, width, DelegateMethods.CutTiles);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 在受击者判定盒内随机位置生成 Excalibur 粒子爆效
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
				new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) },
				Projectile.owner);

			// 手动设置击退方向：远离玩家
			hit.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
				new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) },
				Projectile.owner);

			info.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
		}

		// 复刻原版 Main.DrawProj_Excalibur() 的多层剑光渲染（与 Taurus 完全一致）
		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 position = Projectile.Center - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle sourceRectangle = texture.Frame(1, 4); // 第 1 帧为主剑身
			Vector2 origin = sourceRectangle.Size() / 2f;
			float scale = Projectile.scale * 1.1f;
			SpriteEffects spriteEffects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
			float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
			float lerpTime = Utils.Remap(percentageOfLife, 0f, 0.6f, 0f, 1f) * Utils.Remap(percentageOfLife, 0.6f, 1f, 1f, 0f);
			float lightingColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
			lightingColor = Utils.Remap(lightingColor, 0.2f, 1f, 0f, 1f);

			// #0080FF 蓝色系
			Color backDarkColor = new Color(0, 80, 160);
			Color middleMediumColor = new Color(0, 128, 255);
			Color frontLightColor = new Color(115, 185, 255);

			Color whiteTimesLerpTime = Color.White * lerpTime * 0.5f;
			whiteTimesLerpTime.A = (byte)(whiteTimesLerpTime.A * (1f - lightingColor));
			Color faintLightingColor = whiteTimesLerpTime * lightingColor * 0.5f;
			faintLightingColor.G = (byte)(faintLightingColor.G * lightingColor);
			faintLightingColor.B = (byte)(faintLightingColor.R * (0.25f + lightingColor * 0.75f));

			// 背部深色层
			Main.EntitySpriteDraw(texture, position, sourceRectangle, backDarkColor * lightingColor * lerpTime, Projectile.rotation + Projectile.ai[0] * MathHelper.PiOver4 * -1f * (1f - percentageOfLife), origin, scale, spriteEffects, 0f);
			// 受光照色影响的淡层
			Main.EntitySpriteDraw(texture, position, sourceRectangle, faintLightingColor * 0.15f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
			// 中部中层
			Main.EntitySpriteDraw(texture, position, sourceRectangle, middleMediumColor * lightingColor * lerpTime * 0.3f, Projectile.rotation, origin, scale, spriteEffects, 0f);
			// 前部亮层
			Main.EntitySpriteDraw(texture, position, sourceRectangle, frontLightColor * lightingColor * lerpTime * 0.5f, Projectile.rotation, origin, scale * 0.975f, spriteEffects, 0f);
			// 三条细高光线（第 4 帧）
			Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.6f * lerpTime, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
			Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.5f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.05f, origin, scale * 0.8f, spriteEffects, 0f);
			Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.4f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.1f, origin, scale * 0.6f, spriteEffects, 0f);

			// 沿弧线圆周的 8 颗小星芒
			for (float i = 0f; i < 8f; i += 1f) {
				float edgeRotation = Projectile.rotation + Projectile.ai[0] * i * (MathHelper.Pi * -2f) * 0.025f + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4) * Projectile.ai[0];
				Vector2 drawPos = position + edgeRotation.ToRotationVector2() * ((float)texture.Width * 0.5f - 6f) * scale;
				DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos, new Color(255, 255, 255, 0) * lerpTime * (i / 9f), middleMediumColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, edgeRotation, new Vector2(0f, Utils.Remap(percentageOfLife, 0f, 1f, 3f, 0f)) * scale, Vector2.One * scale);
			}

			// 剑尖处的大星芒
			Vector2 drawPos2 = position + (Projectile.rotation + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4) * Projectile.ai[0]).ToRotationVector2() * ((float)texture.Width * 0.5f - 4f) * scale;
			DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos2, new Color(255, 255, 255, 0) * lerpTime * 0.5f, middleMediumColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(percentageOfLife, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale);

			return false; // 跳过默认绘制
		}

		// 拷贝自原版私有的 Main.DrawPrettyStarSparkle()
		private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness) {
			Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			Color bigColor = shineColor * opacity * 0.5f;
			bigColor.A = 0;
			Vector2 origin = sparkleTexture.Size() / 2f;
			Color smallColor = drawColor * 0.5f;
			float lerpValue = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
			Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X) * lerpValue;
			Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y) * lerpValue;
			bigColor *= lerpValue;
			smallColor *= lerpValue;
			// 亮的大十字
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
			// 暗的小十字
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
		}
	}
}
