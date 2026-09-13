using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.BasePROJ
{
	// ============================================================================
	//  BaseSwordHP —— 围绕前手可编排动作的近战持武器弹幕（本期最重要的地基）
	//
	//  名字保留 Sword 只是便于理解，实际服务剑、斧、大剑、锤、镰等"围绕手部挥动"的近战。
	//
	//  它把一次攻击看成一条由若干动作节点（Aim/Arc/Thrust/Pause/Recover）组成的轨道：
	//  基础类负责插值、姿势、手臂、绘制、伤害窗口、胶囊碰撞、每段独立本地免疫与同步；
	//  子类只提供动作数据（ResolveAction），并可在节点事件里生成自己的冰霜/剑气/音效。
	//
	//  连段：物品在生成弹幕时把"这次用哪个动作编号"写进 ai[0]（普攻由物品用连段计数器
	//  轮换，技能则强制指定），因此基础类内部**不需要**每把武器一套巨大的 switch。
	//
	//  基础类不携带任何固定 VFX 风格，也不内置"某某角色第三击/冻结"等专属判断。
	// ============================================================================
	public abstract class BaseSwordHP : ModProjectile
	{
		// ── 子类提供 ────────────────────────────────────────────────
		protected abstract WeaponSpriteProfile Profile { get; }
		protected abstract MeleeAction ResolveAction(int actionId);
		protected abstract int BindingItemType();
		protected abstract Texture2D GetWeaponTexture();

		// 子类可选钩子
		protected virtual void ModifyStrike(NPC target, ref NPC.HitModifiers modifiers) { }
		protected virtual void OnStrike(NPC target, in NPC.HitInfo hit, int damageDone) { }
		protected virtual void OnActionEvent(int eventId, Vector2 tipWorld) { }

		/// <summary>握点距手部的基础偏移（像素）。挥砍时武器绕这个点旋转。</summary>
		protected virtual float HandOffset => 6f;

		/// <summary>整套动作的额外速度系数（&gt;1 更快、&lt;1 更慢）。子类按 SkillMode 覆写以实现技能加/减速。</summary>
		protected virtual float ActionSpeedScale => 1f;

		protected int ActionId => (int)Projectile.ai[0];
		/// <summary>技能状态：0 普通 / 1 一技能 / 2 二技能。物品生成时写入 ai[1]。</summary>
		protected int SkillMode => (int)Projectile.ai[1];

		private Player Owner => Main.player[Projectile.owner];

		// 运行期
		private float aimAngle;
		private bool faceLeft;
		private float fsign;      // 面朝左时角度偏移取反，保证挥砍观感一致
		private bool initialized;
		// 浮点动作时间允许子类用 extraUpdates 提高真实运动/碰撞采样密度，
		// 而不改变整套动作的实际时长。
		private float age;
		private MeleeAction action;
		private int[] nodeStart;  // 每个节点的起始 tick（含攻速缩放后）
		private int tTotal;
		private int curNode = -1;

		// 当前帧几何
		private Vector2 handWorld;
		private Vector2 tipWorld;
		private float curAngle;
		private bool damaging;
		private float curDamageMul = 1f;
		private float curWidthMul = 1f;
		private bool eventFiredThisNode;

		// 攻击端轨迹（供子类画锐利拖尾）：[0] 为最新；仅在伤害窗口内累积，出窗后逐帧淡出。
		private const int TrailLen = 12;
		private readonly Vector2[] tipTrail = new Vector2[TrailLen];
		private int tipTrailCount;

		// 供子类 VFX 使用的只读状态
		protected Vector2 HandWorld => handWorld;
		protected Vector2 TipWorld => tipWorld;
		protected bool Damaging => damaging;
		protected float CurrentAngle => curAngle;
		protected float AimAngle => aimAngle;
		protected bool FaceLeftPose => faceLeft;
		protected Vector2[] TipTrail => tipTrail;
		protected int TipTrailCount => tipTrailCount;

		/// <summary>子类在此叠加"我们风格"的加法发光/拖尾；基类先画完武器再调用它。默认无。</summary>
		protected virtual void DrawWeaponVfx(Color lightColor) { }
		/// <summary>需要位于武器本体后方的实体刀幕/阴影层。默认无。</summary>
		protected virtual void DrawWeaponVfxBehind(Color lightColor) { }

		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1; // 逐"攻击段"重置（见进入新伤害节点时 ResetLocalNPCHitImmunity）
			Projectile.timeLeft = 600;
		}

		public override bool ShouldUpdatePosition() => false;

		private void Init() {
			Player player = Owner;
			aimAngle = Projectile.velocity.ToRotation();
			faceLeft = BaseHeldMeleeSupport.FaceLeft(aimAngle);
			fsign = faceLeft ? -1f : 1f;

			action = ResolveAction(ActionId);
			// 动作时序 = 全局攻速 × 本次动作的速度系数（ActionSpeedScale）。
			// 连段节奏设计成"由弹幕存活时长决定"（物品把 useTime 压得很低，永远不成为瓶颈），
			// 这样武器全程可见、两次挥砍之间不会闪断；某个技能要更快/更慢，就让子类按 SkillMode
			// 覆写 ActionSpeedScale（例如霜叶 S2 更快、红豆 S2 更慢），无需在基础类里写死技能语义。
			float atkSpeed = player.GetTotalAttackSpeed(Projectile.DamageType) * ActionSpeedScale;

			MeleeNode[] nodes = action.Nodes ?? Array.Empty<MeleeNode>();
			nodeStart = new int[nodes.Length + 1];
			int acc = 0;
			for (int i = 0; i < nodes.Length; i++) {
				nodeStart[i] = acc;
				acc += BaseHeldMeleeSupport.ScaleTicks(nodes[i].Duration, atkSpeed);
			}
			nodeStart[nodes.Length] = acc;
			tTotal = Math.Max(1, acc);

			Projectile.timeLeft = (tTotal + 2) * (Projectile.extraUpdates + 1);
			initialized = true;
		}

		public override void AI() {
			Player player = Owner;

			if (player.dead || !player.active || player.noItems || player.CCed || player.HeldItem.type != BindingItemType()) {
				Projectile.Kill();
				return;
			}

			if (!initialized)
				Init();

			MeleeNode[] nodes = action.Nodes ?? Array.Empty<MeleeNode>();
			if (nodes.Length == 0) {
				Projectile.Kill();
				return;
			}

			// 定位当前节点与节点内进度
			int idx = nodes.Length - 1;
			for (int i = 0; i < nodes.Length; i++) {
				if (age < nodeStart[i + 1]) { idx = i; break; }
			}
			MeleeNode node = nodes[idx];
			int nodeLen = Math.Max(1, nodeStart[idx + 1] - nodeStart[idx]);
			float p = MathHelper.Clamp((age - nodeStart[idx]) / (float)nodeLen, 0f, 1f);
			float e = BaseHeldMeleeSupport.Ease(node.Ease, p);

			// 切换到新节点：若是伤害节点则重置本地免疫（每个攻击段独立记账），并复位事件触发
			if (idx != curNode) {
				curNode = idx;
				eventFiredThisNode = false;
				if (node.Hit.DamageMul > 0f)
					Projectile.ResetLocalNPCHitImmunity();
			}

			// 姿态求解
			float aOff = MathHelper.Lerp(node.A0, node.A1, e) * fsign;
			float reach = MathHelper.Lerp(node.R0, node.R1, e);

			curAngle = aimAngle + aOff;
			Vector2 dir = BaseHeldMeleeSupport.Dir(curAngle);

			// 挥砍时握点大致停在手部；突刺时握点沿瞄准方向前送
			float gripDist = HandOffset + (node.Kind == MeleeNodeKind.Thrust ? reach : 0f);
			handWorld = player.MountedCenter + dir * gripDist;

			// 攻击端长度：基础 CombatReach；挥砍可用 reach 作为额外延伸（重劈更宽更远）
			float weaponLen = Profile.CombatReach * Projectile.scale
				+ (node.Kind == MeleeNodeKind.Arc ? reach : 0f);
			tipWorld = handWorld + dir * weaponLen;

			Projectile.Center = (handWorld + tipWorld) * 0.5f;
			Projectile.velocity = BaseHeldMeleeSupport.Dir(aimAngle);
			Projectile.rotation = curAngle;
			Projectile.spriteDirection = faceLeft ? -1 : 1;

			// 伤害窗口
			damaging = node.Hit.ActiveAt(p);
			curDamageMul = node.Hit.DamageMul <= 0f ? 1f : node.Hit.DamageMul;
			curWidthMul = node.Hit.WidthMul <= 0f ? 1f : node.Hit.WidthMul;

			// 攻击端轨迹累积/淡出
			if (damaging) {
				for (int i = tipTrail.Length - 1; i > 0; i--)
					tipTrail[i] = tipTrail[i - 1];
				tipTrail[0] = tipWorld;
				if (tipTrailCount < tipTrail.Length)
					tipTrailCount++;
			}
			else if (tipTrailCount > 0) {
				tipTrailCount--;
			}

			// 节点事件：纯视觉/音效在各客户端各自触发（动作时序在各端一致，故触发时机同步）；
			// 若子类要在事件里生成弹幕，须自行用 Projectile.owner == Main.myPlayer 判定，避免联机重复。
			if (node.EventId != 0 && !eventFiredThisNode && p >= node.EventProgress) {
				eventFiredThisNode = true;
				OnActionEvent(node.EventId, tipWorld);
			}

			BaseHeldMeleeSupport.HoldAndPose(player, Projectile, handWorld, faceLeft);

			if (age >= tTotal) {
				Projectile.Kill();
				return;
			}
			age += 1f / (Projectile.extraUpdates + 1f);
		}

		public override bool? CanDamage() => damaging ? null : false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!damaging)
				return false;
			if (!BaseHeldMeleeSupport.CapsuleHits(handWorld, tipWorld,
					Profile.CombatWidth * curWidthMul, targetHitbox))
				return false;
			// 突刺节点会把握点向前推；视线必须从玩家本体开始，不能让移动后的握点越过墙体。
			return Collision.CanHit(Owner.MountedCenter, 1, 1,
				targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height);
		}

		public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= curDamageMul;
			ModifyStrike(target, ref modifiers);
		}

		public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnStrike(target, hit, damageDone);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;
			Texture2D tex = GetWeaponTexture();
			DrawWeaponVfxBehind(lightColor);
			BaseHeldMeleeSupport.DrawHeld(tex, handWorld, curAngle, faceLeft, Profile, Projectile.scale, lightColor);
			DrawWeaponVfx(lightColor);
			return false;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(aimAngle);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			aimAngle = reader.ReadSingle();
			faceLeft = BaseHeldMeleeSupport.FaceLeft(aimAngle);
			fsign = faceLeft ? -1f : 1f;
		}
	}
}
