using System.Linq;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Buffs.ArmorSets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public class WisdelShotNormal : ModProjectile
	{
		public override void SetStaticDefaults()
		{
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
		public override void SetDefaults()
		{
            Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
            Projectile.extraUpdates = 2;

		}
		public int timer;
		public bool hasreachedTarget = false;
		public Vector2 aimPos = Vector2.Zero;
		public override void AI()
		{
			timer++;

			Vector2 oldPosition = Projectile.position - Projectile.velocity;
			Vector2 newPosition = Projectile.position;

			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57079637f;
            
			if (hasHit) {
				Projectile.velocity *= 0.5f;
				Projectile.alpha += 10;
				if (Projectile.alpha > 255) {
					Projectile.Kill();
				}
			}
			if (IsPointOnLineSegment(oldPosition, newPosition, aimPos, 16f)) // 16f为容差范围
			{
				Projectile.tileCollide = true;
				hasreachedTarget = true;
			}
		}
		private bool IsPointOnLineSegment(Vector2 start, Vector2 end, Vector2 point, float tolerance) {
			// 计算点到线段的距离
			float distance = DistanceToLineSegment(point, start, end);
			return distance <= tolerance;
		}

		private float DistanceToLineSegment(Vector2 p, Vector2 a, Vector2 b) {
			Vector2 ab = b - a;
			Vector2 ap = p - a;

			float dot = Vector2.Dot(ap, ab);
			float lengthSquared = ab.LengthSquared();

			if (dot <= 0)
				return ap.Length();
			if (dot >= lengthSquared)
				return (p - b).Length();

			Vector2 projection = a + (dot / lengthSquared) * ab;
			return (p - projection).Length();
		}

		public static void Aftershock(Projectile Projectile, Player player, int damage, NPC ignoreNPC = null)
		{
			float radius = 32;

			/*int particleCount = 100;
			for (int i = 0; i < particleCount; i++) {
				float angle = MathHelper.TwoPi * i / particleCount;
				Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius;

				int dust = Dust.NewDust(position, 0, 0, DustID.Torch, 0f, 0f, 100, default, 2f);
				Main.dust[dust].velocity = Vector2.Zero;
				Main.dust[dust].noGravity = true;
			}*/
			foreach (NPC npc in Main.npc) {
				bool immortal = npc.dontTakeDamage || npc.townNPC;
				if (npc != ignoreNPC && npc.active && !immortal)
				{
					//Main.NewText(Vector2.Distance(npc.Center, Projectile.Center));
					if (Vector2.Distance(npc.Center, Projectile.Center) <= radius)
					{
						NPC.HitInfo info = new();
						bool crit = Main.rand.Next(100) < Projectile.CritChance;
						info.Damage = (int)(Projectile.damage * (crit ? 2f : 1f) * Main.rand.NextFloat(0.95f, 1.051f));
						info.Knockback = 0;
						info.HitDirection = (npc.position.X - player.position.X > 0 ? 1 : -1);
						info.Crit = crit;
						info.DamageType = Projectile.DamageType;
						npc.StrikeNPC(info);

						// 维什戴尔套装：残影标记被余震命中时引爆
						if (npc.HasBuff<WisadelMarkDebuff>()) {
							int buffIndex = npc.FindBuffIndex(ModContent.BuffType<WisadelMarkDebuff>());
							if (buffIndex >= 0)
								npc.DelBuff(buffIndex);

							// 直接造成爆炸伤害
							NPC.HitInfo blast = new();
							bool blastCrit = Main.rand.Next(100) < Projectile.CritChance;
							blast.Damage = (int)(Projectile.damage * (blastCrit ? 2f : 1f) * Main.rand.NextFloat(0.95f, 1.051f) * 1.5f);
							blast.Knockback = 0;
							blast.HitDirection = (npc.position.X - player.position.X > 0 ? 1 : -1);
							blast.Crit = blastCrit;
							blast.DamageType = Projectile.DamageType;
							npc.StrikeNPC(blast);

							HitEffect(Projectile);

							// 困惑
							OperatorStunNPC.TryApplyFromWisadel(npc, 60);
						}
					}
				}
			}
		}
		public override void OnKill(int timeLeft)
        {
			if (!hasHit)
			{
				HitEffect(Projectile);
				Aftershock(Projectile, Main.player[Projectile.owner], Projectile.damage);
			}
		}
		public static void HitEffect(Projectile Projectile)
		{
			SoundEngine.PlaySound(Wisdel_Probe.ShootBlast.WithVolumeScale(1.5f), Projectile.position);

			Projectile.NewProjectile
				(new EntitySource_Parent(Projectile), Projectile.Center, Vector2.Zero,
				ModContent.ProjectileType<WisdelHitNormal>(), Projectile.damage / 2,
				Projectile.knockBack, Projectile.owner, ai0: 666);

			for (int i = 0; i < 6; i++) {
				Vector2 newVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30));
				newVelocity *= Main.rand.NextFloat(0.1f, 0.35f) * 3f;
				Vector2 offset = new Vector2(Main.rand.Next(-Projectile.width, Projectile.width),
					Main.rand.Next(-Projectile.height, Projectile.height)) / 8f;
				var p = new DefaultParticleNonPre(Projectile.Center + offset,
				newVelocity, 90, Main.rand.NextFloat(0.35f, 0.5f) * 4f, Color.Black, true);
				p.Deformation = new Vector2(0.1f, 0.6f) * Main.rand.NextFloat(1, 2);
				p.Spawn();
			}
			for (int i = 0; i < 6; i++) {
				Vector2 newVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30));
				newVelocity *= Main.rand.NextFloat(0.1f, 0.35f) * 3f;
				Vector2 offset = new Vector2(Main.rand.Next(-Projectile.width, Projectile.width),
					Main.rand.Next(-Projectile.height, Projectile.height)) / 8f;
				var p = new DefaultParticle(Projectile.Center + offset,
				newVelocity, 90, Main.rand.NextFloat(0.35f, 0.5f) * 4f, new Color(249, 90, 100), true);
				p.Deformation = new Vector2(0.1f, 0.6f) * Main.rand.NextFloat(1, 2);
				p.Spawn();
			}
			for (int i = 0; i < Main.rand.Next(6, 12); i++) {
				Vector2 newVelocity = Vector2.One.RotatedByRandom(MathHelper.ToRadians(360));
				newVelocity *= Main.rand.NextFloat(0.1f, 0.35f) * 10;
				Vector2 offset = new Vector2(Main.rand.Next(-Projectile.width, Projectile.width),
					Main.rand.Next(-Projectile.height, Projectile.height)) / 8f;

				var p = new DefaultParticle(Projectile.Center + offset,
				newVelocity, 90, Main.rand.NextFloat(0.2f, 0.5f) * 3.5f, new Color(249, 90, 100), true);
				p.Deformation = new Vector2(0.35f, 0.4f) * Main.rand.NextFloat(1, 2);
				p.Spawn();
			}
		}
		public override bool? CanDamage()
		{
			return !hasHit;
		}
		public bool hasHit;
		public bool hasCollide;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
			HitEffect(Projectile);
			Aftershock(Projectile, Main.player[Projectile.owner], Projectile.damage, target);
			hasHit = true;
        }
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Color color = Color.HotPink;
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/star_02").Value;
			if (!hasCollide) { 
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
				new Color(color.R, color.G, color.B, Projectile.alpha),
				Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(0.2f, 0.4f), SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			VertexStrip.StripColorFunction colorFunction = (prog) =>
            {
				float lerpValue = Utils.GetLerpValue(0f - 0.1f * transitToDark, 0.7f - 0.2f * transitToDark, prog, true);
				Color result = Color.Lerp(Color.Lerp(Color.HotPink, Color.HotPink, transitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, prog, false));
				result.A /= 8;
				return result * Projectile.Opacity;
			};

            VertexStrip.StripHalfWidthFunction widthFunction = (prog) =>
            {
				return 12f * MathHelper.Lerp(1f, 0.5f, prog / 1.5f);
			};

            var strip = new VertexStrip();


			Main.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
			transitToDark = Utils.GetLerpValue(0f, 6f, 0.1f, true);
			MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
			miscShaderData.UseSaturation(-2f);
			miscShaderData.UseOpacity(50f);
			miscShaderData.Apply(null);
			miscShaderData.UseImage0("Images/Extra_194");
			var rotations = Projectile.oldPos.Zip(Projectile.oldPos.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
			strip.PrepareStrip(Projectile.oldPos, rotations.Prepend(rotations.FirstOrDefault()).ToArray(),
				colorFunction, widthFunction, -Main.screenPosition
				+ new Vector2(Projectile.width, Projectile.height) / 2
				+ new Vector2(-1, 0).RotatedBy(Projectile.rotation));
			strip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			if (hasCollide)
				return;
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
				Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

		}
		private float transitToDark;
	}
}
