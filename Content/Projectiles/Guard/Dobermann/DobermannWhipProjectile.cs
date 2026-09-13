using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Dobermann
{
	public class DobermannWhipProjectile : ModProjectile
	{
		private const string TexRoot = "ArknightsMod/Content/Items/Weapons/Guard/Dobermann/DobermannWhip";

		public override string Texture => TexRoot + "_Hand";

		private static readonly Color LineColor = new Color(0x28, 0xb7, 0xc4);

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private Texture2D _texHand;
		private Texture2D _texSeg;
		private Texture2D _texTip;

		private void EnsureTextures() {
			_texHand ??= ModContent.Request<Texture2D>(TexRoot + "_Hand", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			_texSeg  ??= ModContent.Request<Texture2D>(TexRoot + "_Seg",  ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			_texTip  ??= ModContent.Request<Texture2D>(TexRoot + "_Tip",  ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.WhipSettings.Segments = 14;
			Projectile.WhipSettings.RangeMultiplier = 0.85f;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private bool _firstAI = true;

		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			if (_firstAI) {
				Projectile.WhipSettings.Segments = (int)((owner.whipRangeMultiplier + 1f) * Projectile.WhipSettings.Segments);
				_firstAI = false;
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			List<Vector2> pts = Projectile.WhipPointsForCollision;
			Projectile.FillWhipControlPoints(Projectile, pts);
			Projectile.Center = Vector2.Lerp(Projectile.Center, pts[pts.Count - 1], 1f);
			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			owner.heldProj = Projectile.whoAmI;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
			Timer += 1f;

			if (Timer == swingTime / 2f)
				SoundEngine.PlaySound(SoundID.Item153, GetTipPosition());

			if (Timer >= swingTime || owner.itemAnimation <= 0)
				Projectile.Kill();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			Projectile.damage = (int)(Projectile.damage * 0.8f);
			if (Projectile.damage < 1) Projectile.damage = 1;
		}

		public override bool PreDraw(ref Color lightColor) {
			EnsureTextures();

			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			// 先绘制底层连接线
			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			for (int i = 0; i < list.Count - 1; i++) {
				Texture2D tex;
				float scale = 1f;

				if (i == 0) {
					tex = _texHand;
				} else if (i == list.Count - 2) {
					tex = _texTip;
					Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
					float t = Timer / timeToFlyOut;
					scale = MathHelper.Lerp(0.5f, 1.4f,
						Utils.GetLerpValue(0.1f, 0.7f, t, true) *
						Utils.GetLerpValue(0.9f, 0.7f, t, true));
				} else {
					if (i % 2 != 0) continue;
					tex = _texSeg;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());
				Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

				Main.EntitySpriteDraw(tex, element - Main.screenPosition, null, color, rotation, origin, scale, flip, 0);
			}

			return false;
		}

		private static void DrawLine(List<Vector2> list) {
			Texture2D lineTex = TextureAssets.FishingLine.Value;
			Rectangle frame = lineTex.Frame(1, 1, 0, 0, 0, 0);
			Vector2 origin = new Vector2(frame.Width / 2f, 2f);

			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 2; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), LineColor);
				Vector2 scale = new Vector2(1f, (diff.Length() + 2f) / frame.Height);

				Main.EntitySpriteDraw(lineTex, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
				pos += diff;
			}
		}

		private Vector2 GetTipPosition() {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);
			return list.Count >= 2 ? list[list.Count - 2] : Projectile.Center;
		}
	}
}
