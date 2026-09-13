using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Content.Items.Weapons.Defender.Vulcan;
using ArknightsMod.Content.SwingHelper;
using ArknightsMod.Players;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RuneSKill.Content.NeedTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Vulcan
{
	public class Vulcan_Shield : ModProjectile
	{
		public ShieldHelper helper;
		public Player player => Main.player[Projectile.owner];
		public override void SetDefaults()
		{
			Projectile.width = 10; // ?�������?�����
			Projectile.height = 10; // ?�������?��?�
			Projectile.friendly = true; // ?������?��?���
			Projectile.penetrate = -1; // ?�������?�?
			Projectile.tileCollide = false; // ?���?����?��?
			Projectile.usesLocalNPCImmunity = true; // ?��?�����?
			Projectile.ownerHitCheck = true; // ?��?�����?���������?�����??�?�����?�?��?����?�?
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // ?����?��??����
			Projectile.ignoreWater = true;
			Projectile.localNPCHitCooldown = 11;
		}

		public enum ShieldType {Move,Defender}
		public ShieldType projMode =  ShieldType.Move;
		public override void OnSpawn(IEntitySource source) {
			helper = new ShieldHelper();
		}

		public override void AI() {
			if(player.HeldItem.type != ModContent.ItemType<Vulcan_Weapon>())
				Projectile.Kill();
			Projectile.timeLeft = 2;
			switch (projMode) {
				case ShieldType.Move:
					Move();
					break;
				case ShieldType.Defender:
					Defender();
					break;
			}
		}

		public override bool? CanDamage() {
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
			helper.mp.DrawEffect(TextureAssets.Projectile[Projectile.type].Value);
			sb.End();
			sb.Begin();
			return false;
		}
		private bool press = false;
		public void Move() {
			helper.UpdateMovePose(Projectile, player);
			if (Main.myPlayer == player.whoAmI) {
				if (Main.mouseRight && player.itemTime == 0) {
					projMode = ShieldType.Defender;
				}
			}
		}

		public override bool? CanHitNPC(NPC target) {
			if (projMode == ShieldType.Move)
				return false;
			return true;
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
				projMode =  ShieldType.Move;
			}
			Vector2 fix = new Vector2(1, 0).RotatedBy(attackRad);
			Projectile.Center += fix * Length;
		}
		public void Defender() {
			if (Main.myPlayer == player.whoAmI) {
				if (!Main.mouseRight) {
					projMode = ShieldType.Move;
				}
			}
			helper.UpdateDefenderPose(Projectile, player);
		}

		public Texture2D ShieldTex => TextureAssets.Projectile[Projectile.type].Value;
		public void Draw_Shield(SpriteBatch sb) {
			helper.DrawShield(Projectile, player, ShieldTex, projMode == ShieldType.Defender);
			helper.mp.ParryThemeColor = Color.Black;
			helper.mp.DrawEffect(TextureAssets.Projectile[Projectile.type].Value);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);

		}
	}
}
