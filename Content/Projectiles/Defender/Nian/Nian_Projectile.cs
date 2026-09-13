using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Defender.Nian;
using Terraria.ID;
using ArknightsMod.Players;
using ArknightsMod.Common.VisualEffects;


namespace ArknightsMod.Content.Projectiles.Defender.Nian
{

	public class NianSword : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		Item item => player.HeldItem;
		int attackTime = 0;
		public override bool ShouldUpdatePosition() => false;

		public override void SetDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			Projectile.width = 32;
			Projectile.height = 118;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 45;
			Projectile.hide = false;
		}
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;//拖尾长度
            base.SetStaticDefaults();
        }

		public override bool PreDraw(ref Color lightColor) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			//二技能期间光环
			if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				for (int i = 0; i < 40; i++)
				{
					Vector2 offset = Main.rand.NextVector2Circular(200f, 200f);
					Dust d = Dust.NewDustPerfect(
						player.Center + offset,
						DustID.Blood,
						Vector2.Zero,
						100,
						new Color(240, 20, 20),
						0.8f
					);
					d.noGravity = true;
				}
			}
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Color drawColor = Projectile.GetAlpha(
				Color.Lerp(lightColor, Color.White, 0.7f)
			);
			// 中上原点
			Vector2 origin = new Vector2(-tex.Width/2,0);
			Vector2 center_bias = origin.RotatedBy(Projectile.rotation);
			Main.EntitySpriteDraw(
				tex,
				Projectile.position - Main.screenPosition + center_bias,
				null,
				drawColor,
				Projectile.rotation,
				new Vector2(0,0),
				Projectile.scale,
				SpriteEffects.None,
				0
			);
			// 断琴那copy的拖尾,不知道为什么oldrot有pi/2的差距
			float rangeFix = 64 * Projectile.scale;
			List<Vertex> vertices = [];
			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
				Color coordColor = Main.hslToRgb(0.03f, 1f - i / 15f, 0.5f)*0.7f;
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                if (player.direction == 1)
                {
                    vertices.Add(new Vertex(Projectile.position + center_bias - Main.screenPosition + rangeFix * (Projectile.oldRot[i]+(float)Math.PI/2).ToRotationVector2() * 1.9f,
                      new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 1, 1),coordColor ));//上底
                    vertices.Add(new Vertex(Projectile.position + center_bias - Main.screenPosition + rangeFix *(Projectile.oldRot[i]+(float)Math.PI/2).ToRotationVector2() * 0.25f, 
                        new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type],0,1),coordColor));//下底
               
                }
                else
                {
                    vertices.Add(new Vertex(Projectile.position + center_bias - Main.screenPosition - rangeFix *(Projectile.oldRot[i]-(float)Math.PI/2).ToRotationVector2() * 1.9f,
                                       new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 1, 1), coordColor));//上底
                    vertices.Add(new Vertex(Projectile.position + center_bias - Main.screenPosition - rangeFix *(Projectile.oldRot[i]-(float)Math.PI/2).ToRotationVector2() * 0.25f,
                        new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 0, 1), coordColor));//下底

                }
            }
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Defender/Nian/SlashTex").Value;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Player player = Main.player[Projectile.owner];
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0  && modPlayer.SkillActive){
				modifiers.SourceDamage *= 1.45f;
			}
			else if (modPlayer.Skill == 2  && modPlayer.SkillActive){
				modifiers.SourceDamage *= 2.2f;
			}
		}

		public override void AI() {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (!player.active || player.dead || item.type != ModContent.ItemType<NianWeapon>()) {
				Projectile.Kill();
				return;
			}
			// 二技能期间停止攻击
			if (modPlayer.Skill == 1  && modPlayer.SkillActive){
				Projectile.friendly=false;
				var targetPos = player.Center + new Vector2(30 * player.direction, -75f);
				var toTarget = targetPos - Projectile.position;
				var dashSpeed = 0.2f + toTarget.Length() * 0.1f; // 基础速度 0.2 + 距离系数 0.05
				if (dashSpeed > 20f) dashSpeed = 20f;

				if (toTarget.Length() > 0.5f)
					Projectile.position += Vector2.Normalize(toTarget) * dashSpeed;

				Projectile.rotation = 0;

				//杀戮光环
				float radius = 200f;
				int damage = (int)(Projectile.damage*0.9);
				int hitCooldown = 30; // 每秒一次

				if (++Projectile.ai[1] >= hitCooldown) {

					Projectile.ai[1] = 0;
					NPC.HitInfo info = new();
					bool crit = Main.rand.Next(100) < item.crit;
					info.Damage = (int)(item.damage *0.5* (crit ? 2f : 1f) * Main.rand.NextFloat(0.95f, 1.051f));
					info.Knockback = item.knockBack;
					info.Crit = crit;
					info.DamageType = DamageClass.Magic;
					for (int i = 0; i < Main.maxNPCs; i++) {

						NPC npc = Main.npc[i];

						if (!npc.active || npc.friendly || npc.dontTakeDamage)
							continue;

						float dist = Vector2.Distance(npc.Center, player.Center);

						if (dist <= radius) {
							npc.StrikeNPC(
								info
							);
						}
					}
				}
			}
			else{
				float t = Math.Clamp(Projectile.ai[0] / item.useTime, 0, 1);
				if (t<0.05f){
					Projectile.alpha = 240;
				}
				Projectile.ai[0] += 1;
				if (t <= 0.3f) {
					Projectile.position = player.Center +new Vector2(20*player.direction,-5-5*t/0.3f);
					// Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(0, 0.52f, t / 0.2f);
					Projectile.rotation = player.direction *(float)Math.PI*8/18;
					Projectile.alpha -= 12;
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction *(float)Math.PI*-10/18);
					if (Projectile.alpha<0) Projectile.alpha =0;
				}
				else if (t <= 0.6f) {
					Projectile.position = player.Center +new Vector2(player.direction*(20-40*(t-0.3f)/0.3f),20-30*(t-0.45f)*(t-0.45f)/0.0225f);
					// Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(0, 0.52f, t / 0.2f);
					Projectile.rotation = player.direction *MathHelper.Lerp((float)Math.PI*8/18, (float)Math.PI*46/18, (t-0.3f) / 0.3f);
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction *MathHelper.Lerp((float)Math.PI*-10/18, (float)Math.PI*10/18, (t-0.3f) / 0.3f));
					int count = Main.rand.Next(1, 4);
					Vector2 origin = new Vector2(0,Projectile.height);
					Vector2 center_bias = origin.RotatedBy(Projectile.rotation);
					Vector2 pos = Projectile.position + center_bias;
					for (int i = 0; i < count; i++){
						Dust d = Dust.NewDustPerfect(
							pos,
							DustID.RedTorch,
							Main.rand.NextVector2Circular(2f, 2f),
							Main.rand.Next(30, 120),
							Color.Orange,
							2f
						);

						d.noGravity = true;
					}
				}
				else {
					Projectile.position = player.Center +new Vector2(player.direction*-20,-10);
					Projectile.rotation = player.direction *(float)Math.PI*46/18;
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction *(float)Math.PI*10/18);
					Projectile.alpha +=10;
					if (Projectile.alpha>240)Projectile.alpha=240;
				}
				if (t>0.95f){
					Projectile.ai[0]=0;
				}

			}

			if (player.controlUseItem) {
				Projectile.timeLeft = item.useTime;
			}
		}

		public override bool? Colliding(
			Rectangle projHitbox,
			Rectangle targetHitbox
		)
		{
			// 剑根部（靠近玩家）
			Vector2 start = Projectile.position;

			// 剑尖方向
			Vector2 dir = Projectile.rotation.ToRotationVector2();

			float length = Projectile.height;

			Vector2 end = start + dir * length;

			// 碰撞宽度（相当于剑刃厚度）
			float width = 40f;
			float collisionPoint = 0f;
			// 线段 vs 矩形
			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(),
				targetHitbox.Size(),
				start,
				end,
				width,
				ref collisionPoint
			);
		}

		

	}

	public class NianShield : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		Item item => player.HeldItem;
		
		Vector2 toTarget= new Vector2();
		Vector2 forward = new Vector2(); 
		int attackTime = 0;
		float dashSpeed = 25f;
		float rotdir = 1;

		public override bool ShouldUpdatePosition() => false;

		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 62;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 60;
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12; // 拖尾长度
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Player player = Main.player[Projectile.owner];
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0  && modPlayer.SkillActive){
				modifiers.SourceDamage *= 1.45f;
			}
			else if (modPlayer.Skill == 2  && modPlayer.SkillActive){
				modifiers.SourceDamage *= 2.2f;
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {

			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

			// ===== 拖尾 =====
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				float progress = 1f - i / (float)Projectile.oldPos.Length;

				Color trailColor = new Color(120, 20, 20, 0) * progress * 0.6f;

				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

				Main.EntitySpriteDraw(
					tex,
					drawPos,
					null,
					trailColor,
					Projectile.rotation,
					origin,
					Projectile.scale * (0.9f + progress * 0.3f),
					SpriteEffects.None,
					0
				);
			}

			// ===== 本体 =====
			Main.EntitySpriteDraw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				Projectile.GetAlpha(lightColor),
				Projectile.rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None,
				0
			);

			return false;
		}


		public override void AI() {
			if (!player.active || player.dead || item.type != ModContent.ItemType<NianWeapon>()) {
				Projectile.Kill();
				return;
			}
			Vector2 targetPos;
			Vector2 nextPos;
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			if (!modPlayer.SkillActive){
				targetPos = player.Center + new Vector2(-50 * player.direction, 0);
				toTarget = targetPos - Projectile.Center;
				dashSpeed = 0.2f + toTarget.Length() * 0.1f; // 基础速度 0.2 + 距离系数 0.05
				if (dashSpeed > 20f) dashSpeed = 20f;

				if (toTarget.Length() > 0.5f)
					Projectile.Center += Vector2.Normalize(toTarget) * dashSpeed;
			}
			else{
				if(modPlayer.Skill == 0){
					float t = Math.Clamp(Projectile.ai[0] / (item.useTime*2), 0, 1);

					Projectile.friendly = true;
					if (t < 0.05f && Projectile.ai[1] == 0f){
						forward = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
					}
					if (t < 0.3f && Projectile.ai[1] == 0f) {
						nextPos = Projectile.Center + forward * dashSpeed;
						// —— 是否撞墙（Tile）
						if (Collision.SolidCollision(
							nextPos - Projectile.Size / 4,
							Projectile.width/2,
							Projectile.height/2))
						{
							Projectile.ai[1] = 1f; // 停止
						}
						else {
							Projectile.Center = nextPos;
						}
					}
					/* ================== 0.3 ~ 0.7 ：原地旋转 ================== */
					else if (t < 0.7f) {
						Projectile.ai[1] = 1f;
						Projectile.rotation += 0.4f * player.direction; // 自转
					}

					/* ================== 0.7 ~ 1 ：收回 ================== */
					else {

						Vector2 backTarget = player.Center + new Vector2(-50 * player.direction, 0);
						Vector2 toPlayer = backTarget - Projectile.Center;

						float dist = toPlayer.Length();
						float returnSpeed = MathHelper.Lerp(40f, 18f, (t - 0.7f) / 0.3f);

						if (dist > 20f)
							Projectile.Center += Vector2.Normalize(toPlayer) * returnSpeed;
						else {
							Projectile.Center = backTarget;
							Projectile.ai[1] = 0f; 
							Projectile.ai[0] = 0;
							return;
						}
					}

				
				}
				else if(modPlayer.Skill == 1){
					targetPos = player.Center + new Vector2(30 * player.direction, 0);
					toTarget = targetPos - Projectile.Center;
					dashSpeed = 0.2f + toTarget.Length() * 0.1f;
					if (dashSpeed > 20f) dashSpeed = 20f;

					if (toTarget.Length() > 0.5f)
						Projectile.Center += Vector2.Normalize(toTarget) * dashSpeed;
				}
				else if(modPlayer.Skill == 2){
					float t = Math.Clamp(Projectile.ai[0] / (item.useTime), 0, 1);
					Projectile.friendly = true;
					if (t < 0.05f){
						toTarget = (Main.MouseWorld - Projectile.Center);
						dashSpeed = 20f+toTarget.Length()*0.05f;
						rotdir = Main.rand.NextBool() ? 1f : -1f;
					}
					if (t < 0.5f) {
						nextPos = Projectile.Center + Vector2.Normalize(toTarget) * dashSpeed;
						Projectile.Center = nextPos;
					}
					else {
						Vector2 offset = Projectile.Center - Main.MouseWorld;
						if (offset.LengthSquared() < 1f)
							offset = Vector2.UnitX * 20f;
						float rotateSpeed = 0.03f; // 每帧旋转弧度
						offset = offset.RotatedBy(rotateSpeed * rotdir);

						Projectile.Center = Main.MouseWorld + offset;
					}
					if(t>0.95f){
						Projectile.ai[0] =0;
					}
				}
			}




			Projectile.ai[0] += 1;

			if (player.controlUseItem) {
				Projectile.timeLeft = item.useTime;
			}
		}

		

	}


}