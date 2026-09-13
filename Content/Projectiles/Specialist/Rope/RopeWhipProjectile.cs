using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Rope
{
	// 暗索的钩爪：链鞭类普通攻击弹幕。
	// 仿杜宾的鞭子(DobermannWhipProjectile)，但末端为钩爪、连接为链条，
	// 且命中敌人会把敌人拉向玩家（钩爪武器特性）。
	public class RopeWhipProjectile : ModProjectile
	{
		private const string TexRoot = "ArknightsMod/Content/Items/Weapons/Specialist/Rope/RopeClaw";

		public override string Texture => TexRoot + "_Hand";

		// 由武器在发射后赋值：true 表示这是 S1「勾爪发射」强化的一鞭（更强拉拽）
		public bool Empowered;

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private Texture2D _texHand;
		private Texture2D _texTip;

		private void EnsureTextures() {
			_texHand ??= ModContent.Request<Texture2D>(TexRoot + "_Hand", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			_texTip  ??= ModContent.Request<Texture2D>(TexRoot + "_Tip",  ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.WhipSettings.Segments = 16;
			Projectile.WhipSettings.RangeMultiplier = 1.1f;
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

			// 沿鞭身末端的挥击路径留下紫色发光粒子，每帧在末端及其前一节各生成一颗，兼顾密度与轨迹宽度
			for (int i = pts.Count - 2; i < pts.Count; i++) {
				Dust d = Dust.NewDustPerfect(pts[i], DustID.PurpleTorch, Vector2.Zero, 0, default, 1.3f);
				d.noGravity = true;
				d.fadeIn = 0.7f;
			}

			owner.heldProj = Projectile.whoAmI;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
			Timer += 1f;

			if (Timer == swingTime / 2f)
				SoundEngine.PlaySound(SoundID.Item153, GetTipPosition());

			if (Timer >= swingTime || owner.itemAnimation <= 0)
				Projectile.Kill();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Player owner = Main.player[Projectile.owner];
			owner.MinionAttackTargetNPC = target.whoAmI;

			// 钩爪特性：把命中的敌人拉向玩家（拉到近处）
			if (!target.boss && target.knockBackResist > 0f) {
				Vector2 pull = (owner.Center - target.Center).SafeNormalize(Vector2.Zero);
				float strength = Empowered ? 17f : 9f; // S1 强化时拉得更狠
				target.velocity = pull * strength;
				target.netUpdate = true;

				int n = Empowered ? 9 : 4;
				for (int k = 0; k < n; k++) {
					Dust d = Dust.NewDustPerfect(target.Center, DustID.GemRuby,
						pull * Main.rand.NextFloat(2f, 5f), 0, default, 1.15f);
					d.noGravity = true;
				}
			}

			// 标签伤害随多段命中递减（仿杜宾鞭）
			Projectile.damage = (int)(Projectile.damage * 0.8f);
			if (Projectile.damage < 1) Projectile.damage = 1;
		}

		public override bool PreDraw(ref Color lightColor) {
			EnsureTextures();

			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			// 链条（鞭身）
			DrawChain(list);

			SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			// 握把：起点
			{
				Vector2 element = list[0];
				Vector2 diff = list[1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());
				Vector2 origin = new Vector2(_texHand.Width / 2f, _texHand.Height / 2f);
				Main.EntitySpriteDraw(_texHand, element - Main.screenPosition, null, color, rotation, origin, 1f, flip, 0);
			}

			// 钩爪：末端，带挥出放大的弹性效果
			{
				int last = list.Count - 2;
				Vector2 element = list[last];
				Vector2 diff = list[last + 1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());
				Vector2 origin = new Vector2(_texTip.Width / 2f, _texTip.Height / 2f);

				Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
				float t = Timer / timeToFlyOut;
				float scale = MathHelper.Lerp(0.6f, 1.5f,
					Utils.GetLerpValue(0.1f, 0.7f, t, true) *
					Utils.GetLerpValue(0.9f, 0.7f, t, true));

				Main.EntitySpriteDraw(_texTip, element - Main.screenPosition, null, color, rotation, origin, scale, flip, 0);
			}

			return false;
		}

		// 用原版链条贴图绘制鞭身
		private static void DrawChain(List<Vector2> list) {
			Texture2D chain = TextureAssets.Chain.Value;
			Vector2 origin = new Vector2(chain.Width / 2f, chain.Height / 2f);

			for (int i = 0; i < list.Count - 2; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());
				Vector2 scale = new Vector2(1f, (diff.Length() + 2f) / chain.Height);
				Main.EntitySpriteDraw(chain, element - Main.screenPosition, null, color, rotation, origin, scale, SpriteEffects.None, 0);
			}
		}

		private Vector2 GetTipPosition() {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);
			return list.Count >= 2 ? list[list.Count - 2] : Projectile.Center;
		}
	}
}
