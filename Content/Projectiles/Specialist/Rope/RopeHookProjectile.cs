using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ArknightsMod.Content.Projectiles.Specialist.Rope
{
	// 暗索的勾爪：抓钩弹幕。
	// 右键技能发射 → 飞出命中第一个敌人造成伤害 → 把敌人拖拽到玩家面前 → 收回自毁。
	// S1 单钩、S2 双钩（各自独立飞行、抓取各自路径上的第一个敌人）。
	public class RopeHookProjectile : ModProjectile
	{
		// 复用钩链爪尖贴图作为钩头，外观与鞭子统一
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Specialist/Rope/RopeClaw_Tip";

		// 状态机：0 伸出 / 1 拖拽 / 2 收回
		private ref float State => ref Projectile.ai[0];
		// 被勾住的 NPC（存 whoAmI+1，0 表示无）
		private ref float Hooked => ref Projectile.ai[1];

		private const float MaxReach = 820f;   // 最大伸出距离
		private const int MaxReelTime = 45;    // 拖拽最长帧数（兜底）
		private const float RetractSpeed = 26f; // 收回速度

		private int reelTimer;

		private Player Owner => Main.player[Projectile.owner];

		public override void SetDefaults() {
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;            // 仅伸出阶段造成接触伤害
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;             // 命中后不消失，改为拖拽
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;   // 每个敌人最多被同一钩命中一次
			Projectile.tileCollide = false;        // 穿过地形，确保能勾到敌人
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			Player owner = Owner;
			if (!owner.active || owner.dead) { Projectile.Kill(); return; }

			switch ((int)State) {
				case 0: ExtendPhase(owner); break;
				case 1: ReelPhase(owner); break;
				default: RetractPhase(owner); break;
			}

			// 钩头朝向：伸出/收回时朝速度方向，拖拽时朝玩家
			if (State == 1)
				Projectile.rotation = (owner.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2;
			else
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.35f, 0.12f, 0.12f);

			// 在飞出/收回的攻击路径上留下紫色发光粒子，每帧一颗
			if (State != 1) {
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Vector2.Zero, 0, default, 1.4f);
				d.noGravity = true;
				d.fadeIn = 0.7f;
			}
		}

		// 伸出：直线飞行，超出射程则收回（命中交给 OnHitNPC 处理）
		private void ExtendPhase(Player owner) {
			if (Vector2.Distance(Projectile.Center, owner.Center) >= MaxReach) {
				BeginRetract();
			}
		}

		// 拖拽：钩头贴住敌人，把敌人拽向玩家面前
		private void ReelPhase(Player owner) {
			int idx = (int)Hooked - 1;
			if (idx < 0 || idx >= Main.maxNPCs || !Main.npc[idx].active) {
				BeginRetract();
				return;
			}

			NPC npc = Main.npc[idx];
			Projectile.Center = npc.Center;
			Projectile.velocity = Vector2.Zero;

			Vector2 frontPos = owner.Center + new Vector2(owner.direction * 44f, -10f);

			// Boss 与不可击退单位不被强拽，仅保留伤害
			if (!npc.boss && npc.knockBackResist > 0f) {
				npc.Center = Vector2.Lerp(npc.Center, frontPos, 0.22f);
				npc.velocity *= 0.6f;
				npc.netUpdate = true;

				// 拖拽尘埃
				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(npc.Center, DustID.GemRuby, (owner.Center - npc.Center) * 0.05f, 0, default, 1.1f).noGravity = true;
			}

			reelTimer++;
			if (reelTimer >= MaxReelTime || Vector2.Distance(npc.Center, owner.Center) < 64f)
				BeginRetract();
		}

		// 收回：飞回玩家，到达后自毁
		private void RetractPhase(Player owner) {
			Vector2 toOwner = owner.Center - Projectile.Center;
			if (toOwner.Length() < 32f) { Projectile.Kill(); return; }
			Projectile.velocity = toOwner.SafeNormalize(Vector2.Zero) * RetractSpeed;
		}

		private void BeginRetract() {
			State = 2;
			Projectile.friendly = false; // 收回不再误伤其他敌人
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (State != 0) return;       // 仅伸出阶段抓取
			Hooked = target.whoAmI + 1;
			State = 1;
			reelTimer = 0;
			Projectile.friendly = false;  // 抓住后不再对其他敌人造成接触伤害
			SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor) {
			DrawChain();
			return true; // 继续绘制默认钩头贴图
		}

		// 绘制玩家到钩头之间的链条
		private void DrawChain() {
			Texture2D chain = TextureAssets.Chain.Value;
			Vector2 start = Owner.MountedCenter;
			Vector2 end = Projectile.Center;
			Vector2 diff = end - start;
			float rot = diff.ToRotation() - MathHelper.PiOver2;
			float dist = diff.Length();
			if (dist < 1f) return;

			int segH = chain.Height;
			int count = (int)(dist / segH) + 1;
			Vector2 step = diff.SafeNormalize(Vector2.Zero) * segH;
			Vector2 pos = start;
			for (int i = 0; i < count; i++) {
				Color c = Lighting.GetColor(pos.ToTileCoordinates());
				Main.EntitySpriteDraw(chain, pos - Main.screenPosition, null, c, rot, chain.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				pos += step;
			}
		}
	}
}
