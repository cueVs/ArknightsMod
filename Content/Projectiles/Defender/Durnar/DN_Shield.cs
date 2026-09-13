using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Players;
using ArknightsMod.Content.SwingHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Durnar
{
	public class DN_Shield : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		Item item => player.HeldItem;
		private Texture2D ShieldTex {
			get {
				if (projMode == ProjMode.Attack)
					return ModContent.Request<Texture2D>($"{Texture}_Attack").Value;
				return TextureAssets.Projectile[ModContent.ProjectileType<DN_Shield>()].Value;
				;
			}
		}
		private readonly ShieldHelper shieldHelper = new();

		public override void SetDefaults() {
			shieldHelper.SetDefaults(Projectile, (int)attackMaxTime + 1);
		}
		private ProjMode projMode = ProjMode.Move;
		public override void AI() {
			Projectile.damage = item.damage;
			if (player.dead || !player.active || item.type != ModContent.ItemType<DN_Weapon>())
				Projectile.Kill();
			Projectile.timeLeft = 2;
			switch (projMode) {
				case ProjMode.Move:
					Move();
					break;
				case ProjMode.Defender:
					Defender();
					break;
				case ProjMode.Attack:
					Attack();
					break;
			}
		}

		public override bool? CanDamage() {
			if (projMode == ProjMode.Attack)
				return true;
			return false;
		}

		private float attackRad;
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch sb = Main.spriteBatch;
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				SamplerState.AnisotropicClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Draw_Shield(sb);
			shieldHelper.mp.ParryThemeColor = Color.Purple;
			shieldHelper.mp.DrawEffect(TextureAssets.Projectile[Projectile.type].Value);
			sb.End();
			sb.Begin();
			return false;
		}
		private bool press = false;
		public void Move() {
			shieldHelper.UpdateMovePose(Projectile, player);
			if (Main.myPlayer == player.whoAmI) {
				if (Main.mouseRight && player.itemTime == 0) {
					projMode = ProjMode.Defender;
				}
				var modPlayer = player.GetModPlayer<DNProj_Player>();
				if (modPlayer.ShieldAttackMode && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && !press) {
					press = true;
					attackRad = MathF.Atan2((Main.MouseWorld - player.MountedCenter).Y, (Main.MouseWorld - player.MountedCenter).X);
					projMode = ProjMode.Attack;
					player.direction = (Main.MouseWorld - player.MountedCenter).X >= 0 ? 1 : -1;
					attackTime = 0;
					CDTime = CDTimeMax;
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.SkillActive)
				modifiers.SourceDamage *= 1.8f;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.SkillActive)
				modifiers.SourceDamage *= 1.8f;
		}

		private float attackTime;
		private float attackMaxTime = 20;

		private float CDTime;

		private float CDTimeMax = 10;
		public void Attack() {
			float progress = attackTime / attackMaxTime;
			float prog2 = 1;
			Projectile.rotation = attackRad - MathHelper.Pi / 2 + MathHelper.Pi / 2 * player.direction;
			float mineRad = Projectile.rotation - MathHelper.Pi;
			float accelProgress = progress * progress;
			float Length = MathHelper.Lerp(-20, 30, accelProgress);
			if (progress <= 1f)
				attackTime++;
			else {
				CDTime--;
				prog2 = CDTime / CDTimeMax;
				Length = MathHelper.Lerp(0, Length, prog2);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation);
			}
			if (progress < 0.4) {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, mineRad);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, mineRad);
			}
			else if (progress <= 1f) {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation);
			}
			if (prog2 < 0) {
				press = false;
				projMode = ProjMode.Move;
			}
			Vector2 fix = new Vector2(1, 0).RotatedBy(attackRad);
			Projectile.Center += fix * Length;
		}
		public void Defender() {
			if (Main.myPlayer == player.whoAmI) {
				if (!Main.mouseRight) {
					projMode = ProjMode.Move;
				}
			}
			shieldHelper.UpdateDefenderPose(Projectile, player);
		}

		public void Draw_Shield(SpriteBatch sb) {
			shieldHelper.DrawShield(Projectile, player, ShieldTex, projMode == ProjMode.Defender);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);

		}
	}
}
