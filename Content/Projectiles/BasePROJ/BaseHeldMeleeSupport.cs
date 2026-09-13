using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.BasePROJ
{
	// ============================================================================
	//  持握近战弹幕的「共享内部支持层」
	//
	//  这里只放两套基础弹幕（BaseLanceHP / BaseSwordHP）都要用、而单把武器不应该
	//  各写一遍的通用工具：外形档案、握点绘制、左右镜像、前手姿势、缓动曲线、
	//  胶囊线碰撞、以及一个轻量的连段计数器。
	//
	//  它**不是**第三套「万能武器基类」——业务武器不要直接继承这里的任何东西，
	//  而是继承 BaseLanceHP 或 BaseSwordHP，由那两个基类来调用这里的静态工具。
	//
	//  设计约束（来自交接文档）：
	//   · 专用服务器不加载 Texture2D，所以一切"伤害/碰撞"只能用 CombatReach /
	//     CombatWidth 与动作数据推导，绝不能依赖贴图像素尺寸。贴图尺寸只在客户端
	//     绘制时使用。
	//   · 不引用、不复制任何 CalamityMod 代码；这里全部基于 tModLoader 原生公开 API。
	// ============================================================================

	/// <summary>缓动类型。首版只提供有限枚举，保证可调试、可同步、可维护。</summary>
	public enum MeleeEase
	{
		Linear,
		Smooth,   // 平滑（smoothstep）
		EaseIn,   // 缓入
		EaseOut,  // 缓出
		FastOut,  // 迅速释放后自然减速（长柄重扫）
		Heavy     // 重击式缓入缓出（起手慢、收尾干脆）
	}

	/// <summary>动作节点的种类。</summary>
	public enum MeleeNodeKind
	{
		Aim,     // 举起/朝向目标，把武器带到下一动作的起始姿势（无伤害）
		Arc,     // 围绕手部按弧线挥动（剑/斧/大剑/锤的挥砍）
		Thrust,  // 沿瞄准方向直线前送（刺）
		Pause,   // 短暂停顿（通常无伤害，用来表达重击蓄势）
		Recover  // 收招/衔接下一击（无伤害）
	}

	/// <summary>
	/// 外形档案：每把武器声明一份少量数据，让基础类能在不同贴图尺寸/左右镜像下
	/// 自动把握点贴合前手、把攻击端算对。归一化锚点 0–1 只影响绘制；伤害长度/宽度
	/// 由 CombatReach / CombatWidth 单独给出，不从贴图推导（专用服务器可靠性）。
	/// </summary>
	public struct WeaponSpriteProfile
	{
		/// <summary>前手握点在贴图中的归一化位置（0–1）。</summary>
		public Vector2 GripAnchor;
		/// <summary>主要攻击端（枪尖/斧刃外沿）在贴图中的归一化位置（0–1），仅用于绘制拖尾等。</summary>
		public Vector2 TipAnchor;
		/// <summary>双手武器的后手位置（可空，首版可不填）。</summary>
		public Vector2? BackGripAnchor;
		/// <summary>锚点重合时使用的原生朝向后备值；正常绘制会从握点与攻击端自动推导。</summary>
		public float NativeForwardAngle;
		/// <summary>视觉比例，默认 1。</summary>
		public float DrawScale;
		/// <summary>服务器使用的攻击长度（世界像素，握点→攻击端）。不能由贴图尺寸推导。</summary>
		public float CombatReach;
		/// <summary>服务器使用的判定宽度（世界像素）。不能由贴图宽度推导。</summary>
		public float CombatWidth;

		public WeaponSpriteProfile(Vector2 grip, Vector2 tip, float nativeForwardAngle,
			float combatReach, float combatWidth, float drawScale = 1f, Vector2? backGrip = null) {
			GripAnchor = grip;
			TipAnchor = tip;
			NativeForwardAngle = nativeForwardAngle;
			CombatReach = combatReach;
			CombatWidth = combatWidth;
			DrawScale = drawScale;
			BackGripAnchor = backGrip;
		}
	}

	/// <summary>
	/// 伤害窗口：以节点内进度（0–1）描述"何时能打人"，与视觉尺寸无关。
	/// DamageMul 为 0 表示该节点不造成伤害。
	/// </summary>
	public struct MeleeHitWindow
	{
		public float Start;      // 起始进度
		public float End;        // 结束进度
		public float DamageMul;  // 伤害倍率（0 = 无伤害）
		public float WidthMul;   // 判定宽度倍率（相对 CombatWidth）

		public static readonly MeleeHitWindow None = new() { Start = 0f, End = 0f, DamageMul = 0f, WidthMul = 1f };

		public static MeleeHitWindow Window(float start, float end, float damageMul, float widthMul = 1f)
			=> new() { Start = start, End = end, DamageMul = damageMul, WidthMul = widthMul };

		public readonly bool ActiveAt(float p) => DamageMul > 0f && p >= Start && p <= End;
	}

	/// <summary>
	/// 单个动作节点：只描述"从什么姿势到什么姿势、持续多久、何时能伤害"。
	/// 角度以"相对瞄准方向的偏移"（弧度）表示；伸出距离为世界像素。
	/// </summary>
	public struct MeleeNode
	{
		public MeleeNodeKind Kind;
		public int Duration;        // 基础时长（tick，未乘攻速）
		public float A0, A1;        // 角度偏移区间（相对瞄准方向）
		public float R0, R1;        // 伸出/半径区间（像素）
		public MeleeEase Ease;
		public bool LockAim;        // 是否锁定为动作起始时采样的瞄准方向
		public MeleeHitWindow Hit;
		public int EventId;         // 子类事件标识（0 = 无事件）
		public float EventProgress; // 事件触发进度
	}

	/// <summary>一整套攻击动作：若干节点 + 连段后继编号。</summary>
	public struct MeleeAction
	{
		public int Id;
		public MeleeNode[] Nodes;
		public int NextCombo; // 连段后继动作编号（-1 = 回到 0 / 结束）
	}

	internal static class BaseHeldMeleeSupport
	{
		// ── 连段计数器 ───────────────────────────────────────────────
		// 只在武器所属客户端（发起攻击的那台）写入/读取；结果通过生成弹幕时写进 ai 同步给
		// 其它客户端，所以这里不需要任何联机处理。用帧计数判断"上一击过去多久"，超时自动归零。
		private static readonly int[] _comboStep = new int[Main.maxPlayers + 1];
		private static readonly uint[] _comboFrame = new uint[Main.maxPlayers + 1];
		private static readonly int[] _comboItemType = new int[Main.maxPlayers + 1];

		/// <summary>取当前连段序号并推进到下一段；两次攻击间隔超过 resetFrames 帧则从 0 重新开始。
		/// 默认 60 帧（比最慢的一段还宽），避免慢攻速下连段被中途误重置。</summary>
		internal static int NextCombo(Player player, int steps, uint resetFrames = 60) {
			steps = Math.Max(1, steps);
			int who = player.whoAmI;
			if ((uint)who >= _comboStep.Length)
				return 0;

			uint now = Main.GameUpdateCount;
			int itemType = player.HeldItem.type;
			if (_comboItemType[who] != itemType || now - _comboFrame[who] > resetFrames)
				_comboStep[who] = 0;
			int s = _comboStep[who] % steps;
			_comboStep[who] = (s + 1) % steps;
			_comboFrame[who] = now;
			_comboItemType[who] = itemType;
			return s;
		}

		// ── 基础数学 ────────────────────────────────────────────────
		internal static Vector2 Dir(float angle) => new(MathF.Cos(angle), MathF.Sin(angle));

		/// <summary>瞄准方向指向左半边（cos&lt;0）时视为面朝左，用于统一镜像判定。</summary>
		internal static bool FaceLeft(float aimAngle) => MathF.Cos(aimAngle) < 0f;

		/// <summary>按攻速把基础时长换算为实际帧数（攻速取自玩家，所有客户端一致，故动作时序天然同步）。</summary>
		internal static int ScaleTicks(int baseTicks, float attackSpeed) =>
			Math.Max(1, (int)Math.Round(baseTicks / Math.Max(0.1f, attackSpeed)));

		internal static float Ease(MeleeEase e, float t) {
			t = MathHelper.Clamp(t, 0f, 1f);
			return e switch {
				MeleeEase.Linear => t,
				MeleeEase.Smooth => t * t * (3f - 2f * t),
				MeleeEase.EaseIn => t * t,
				MeleeEase.EaseOut => 1f - (1f - t) * (1f - t),
				MeleeEase.FastOut => 1f - MathF.Pow(1f - t, 2.6f),
				MeleeEase.Heavy => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
				_ => t
			};
		}

		// ── 碰撞 ───────────────────────────────────────────────────
		/// <summary>握点→攻击端 的粗线段（胶囊）是否命中目标矩形。纯几何，服务器安全。</summary>
		internal static bool CapsuleHits(Vector2 handWorld, Vector2 tipWorld, float width, Rectangle target) {
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(
				target.TopLeft(), target.Size(), handWorld, tipWorld, MathF.Max(1f, width), ref collisionPoint);
		}

		// ── 姿势 / 手臂 ─────────────────────────────────────────────
		/// <summary>
		/// 让玩家保持"正在挥这把弹幕"的持握姿势：接管前手视觉、面朝瞄准方向。
		/// 前手对准 手部→握点 的方向；改错这里只会影响手臂视觉，不影响伤害。
		///
		/// 关键：**不要**在这里钉 <c>player.itemTime/itemAnimation</c>。本套武器用
		/// autoReuse + <c>ownedProjectileCounts&lt;1</c> 控制连段节奏，钉住物品动画会让原版
		/// 每帧不断尝试重新使用、把 <c>CanUseItem</c> 变成每帧调用——任何写在 CanUseItem 里的
		/// 副作用（例如攻击充能）都会被放大几十倍。持握视觉只靠 heldProj + 复合前手维持即可，
		/// 二者都与 itemAnimation 无关，每帧重设不会闪断。
		/// </summary>
		internal static void HoldAndPose(Player player, Projectile proj, Vector2 gripWorld, bool faceLeft) {
			player.heldProj = proj.whoAmI;
			player.ChangeDir(faceLeft ? -1 : 1);

			// 复合前手：0 度朝正下方，故指向 φ 方向时旋转 = φ - PiOver2。
			float armRot = (gripWorld - player.MountedCenter).ToRotation() - MathHelper.PiOver2;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
		}

		// ── 绘制 ───────────────────────────────────────────────────
		/// <summary>
		/// 以握点为原点绘制武器贴图，使握点始终贴合前手。面朝左时纵向翻转并镜像握点/朝向基准，
		/// 保证攻击端仍然指向 currentAngle、上下不颠倒。攻击端方向、碰撞另行按 currentAngle 计算，
		/// 与贴图无关——这样翻转只影响观感，不会改变判定。
		/// </summary>
		internal static void DrawHeld(Texture2D tex, Vector2 gripWorld, float currentAngle,
			bool faceLeft, in WeaponSpriteProfile p, float scale, Color light) {
			if (tex == null)
				return;

			int w = tex.Width;
			int h = tex.Height;
			Vector2 originPx = new(p.GripAnchor.X * w, p.GripAnchor.Y * h);
			Vector2 tipPx = new(p.TipAnchor.X * w, p.TipAnchor.Y * h);
			Vector2 nativeAxis = tipPx - originPx;
			// 贴图锚点才是可靠事实。用真实像素宽高计算“握点→攻击端”的方向，
			// 既能处理非正方形贴图，也不会再因为手填角度与锚点相互矛盾而整把武器翻转。
			float nativeForwardAngle = nativeAxis.LengthSquared() > 0.001f
				? nativeAxis.ToRotation()
				: p.NativeForwardAngle;
			SpriteEffects fx = SpriteEffects.None;
			float rot;

			if (!faceLeft) {
				rot = currentAngle - nativeForwardAngle;
			}
			else {
				// 纵向翻转后，贴图 握点→攻击端 的 Y 分量取反，等效朝向基准变为 +NativeForwardAngle；
				// 同步把 Y 原点移到对侧边缘，握点才不会跳位。
				fx = SpriteEffects.FlipVertically;
				originPx.Y = h - originPx.Y;
				rot = currentAngle + nativeForwardAngle;
			}

			Main.spriteBatch.Draw(tex, gripWorld - Main.screenPosition, null, light, rot,
				originPx, scale * p.DrawScale, fx, 0f);
		}

		// ── 轻量 VFX 助手（供子类叠加"我们风格"的加法发光/拖尾，基类本身不绑定任何风格）──
		//   风格约定：加法混合 + 柔光叠层 + MagicPixel 拉伸的锐利 streak，克制、不糊。

		/// <summary>把当前 SpriteBatch 切到加法混合（画完必须调 EndAdditive 还原）。</summary>
		internal static void BeginAdditive(SpriteBatch sb) {
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
				DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>还原到弹幕默认的 AlphaBlend 批次，避免影响后续弹幕绘制。</summary>
		internal static void EndAdditive(SpriteBatch sb) {
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
				DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// 沿一串世界坐标点画一条渐细渐隐的锐利拖尾（MagicPixel 拉伸的带状）。pts[0] 为最新（最亮最宽）。
		/// 用 sourceRect=(0,0,1,1) 明确取单像素，规避 MagicPixel 可能位于图集的问题。
		/// </summary>
		internal static void DrawStreak(Texture2D pixel, Vector2[] pts, int count, Color inner, Color outer, float width) {
			if (pixel == null || count < 2)
				return;
			var src = new Rectangle(0, 0, 1, 1);
			var origin = new Vector2(0f, 0.5f);
			for (int i = 0; i < count - 1; i++) {
				Vector2 a = pts[i] - Main.screenPosition;
				Vector2 b = pts[i + 1] - Main.screenPosition;
				Vector2 d = b - a;
				float len = d.Length();
				if (len < 0.5f)
					continue;
				float head = 1f - i / (float)(count - 1); // 头部=1，尾部=0
				Color c = Color.Lerp(inner, outer, 1f - head) * (head * head);
				Main.spriteBatch.Draw(pixel, a, src, c, d.ToRotation(), origin,
					new Vector2(len, MathF.Max(1f, width * head)), SpriteEffects.None, 0f);
			}
		}

		/// <summary>在世界坐标处画一团柔光（加法）。</summary>
		internal static void DrawGlow(Texture2D glow, Vector2 world, Color color, float scale, float rotation = 0f) {
			if (glow == null)
				return;
			Main.spriteBatch.Draw(glow, world - Main.screenPosition, null, color, rotation,
				glow.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// 用原版 98 号 SharpTears 绘制统一的撕裂光痕。调用者传入世界长度与宽度，
		/// 不直接依赖素材像素尺寸，避免再次把多帧图集按整张贴到屏幕上。
		/// </summary>
		internal static void DrawSharpTear(Vector2 world, float rotation, Color color,
			float length, float width, float opacity = 1f, SpriteEffects effects = SpriteEffects.None) {
			if (Main.dedServ || length <= 0f || width <= 0f || opacity <= 0f)
				return;

			Texture2D tear = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			// SharpTears 的原生长轴是竖直方向；把目标宽度映射到 X、目标长度映射到 Y，
			// 再额外旋转 90 度，使调用方仍可用常规的“朝向角”描述光痕方向。
			Vector2 scale = new(width / Math.Max(1f, tear.Width), length / Math.Max(1f, tear.Height));
			Main.spriteBatch.Draw(tear, world - Main.screenPosition, null,
				color * MathHelper.Clamp(opacity, 0f, 1f), rotation + MathHelper.PiOver2, tear.Size() * 0.5f,
				scale, effects, 0f);
		}
	}
}
