using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Shirayuki
{
	public class Shirayuki_Shuriken_Proj : ModProjectile
	{
		public ref float Skill => ref Projectile.ai[0];

		private int skill1TimeLeft;
		private bool skill1Bool = false;
		private bool skill2Bool = false;

		public override void SetStaticDefaults() { }

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;

			skill1TimeLeft = (int)(Projectile.timeLeft * 1.4);
		}

		public override void AI()
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Projectile.rotation += MathHelper.Pi / 10f;

			Projectile.localAI[0] = Projectile.rotation * 1.5f;

			if (modPlayer.Skill == 0 && modPlayer.SkillActive)
			{
				if (!skill1Bool)
				{
					Projectile.timeLeft = skill1TimeLeft;
					skill1Bool = true;
				}

			}
			else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				if (!skill2Bool && Projectile.timeLeft <= 1)
				{
					Projectile.timeLeft = 60;
					skill2Bool = true;

					Projectile.tileCollide = false;
				}

			}
			else
			{

			}

		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			for (int i = 0; i < 6; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Flare);
				dust.noGravity = true;
				dust.velocity *= 4f;
				dust.scale *= 2f;

				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Firework_Yellow);
				dust2.noGravity = true;
				dust2.velocity *= 2f;
				dust2.scale *= 0.5f;
			}

			if (modPlayer.Skill == 1 && modPlayer.SkillActive && !skill2Bool)
			{
				Projectile.timeLeft = 60;
				skill2Bool = true;

				Projectile.tileCollide = false;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 6; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Flare);
				dust.noGravity = true;
				dust.velocity *= 4f;
				dust.scale *= 2f;

				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Firework_Yellow);
				dust2.noGravity = true;
				dust2.velocity *= 2f;
				dust2.scale *= 0.5f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 1 && modPlayer.SkillActive && !skill2Bool)
			{
				Projectile.timeLeft = 60;
				skill2Bool = true;

				Projectile.tileCollide = false;

				return false;
			}

			return base.OnTileCollide(oldVelocity);
		}

		public override bool ShouldUpdatePosition()
		{
			if (skill2Bool)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() * 0.5f;

			Main.EntitySpriteDraw(texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None,
				0
				);

			if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				Texture2D texture1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Shirayuki/Shirayuki_Shuriken_Effect1", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Texture2D texture2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Shirayuki/Shirayuki_Shuriken_Effect2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Texture2D texture3 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Shirayuki/Shirayuki_Shuriken_Effect3", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Main.EntitySpriteDraw(texture1,
					Projectile.Center - Main.screenPosition,
					null,
					Color.White,
					Projectile.rotation + MathHelper.Pi / 3,
					texture1.Size() * 0.5f,
					Projectile.scale * 0.5f,
					SpriteEffects.None,
					0
					);
				Main.EntitySpriteDraw(texture2,
					Projectile.Center - Main.screenPosition,
					null,
					new Color(255, 174, 26),
					Projectile.rotation,
					texture2.Size() * 0.5f,
					Projectile.scale * 0.5f,
					SpriteEffects.None,
					0
					);
				Main.EntitySpriteDraw(texture3,
					Projectile.Center - Main.screenPosition,
					null,
					new Color(255, 174, 26),
					Projectile.localAI[0] - MathHelper.PiOver4 / 3,
					texture3.Size() * 0.5f,
					Projectile.scale * 0.5f,
					SpriteEffects.None,
					0
					);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}

		//模组目前不考虑多人，这些貌似目前不需要（？）
		/*
		public override bool CanHitPlayer(Player target)
		{
			if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
				return false;

			return base.CanHitPlayer(target);
		}

		public override bool CanHitPvp(Player target)
		{
			if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
				return false;

			return base.CanHitPvp(target);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			for (int i = 0; i < 6; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Flare);
				dust.noGravity = true;
				dust.velocity *= 4f;
				dust.scale *= 2f;

				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Firework_Yellow);
				dust2.noGravity = true;
				dust2.velocity *= 2f;
				dust2.scale *= 0.5f;
			}

			if (Skill == 2 && !skill2Bool) {
				Projectile.timeLeft = 60;
				skill2Bool = true;

				Projectile.tileCollide = false;
			}
		}
		*/
	}
}
