using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4.RaptorDrone;

public class RaptorDrone : ModNPC
{
	private const string TextureRoot = "ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/RaptorDrone/RaptorDrone";

	private const int IdleDuration = 5 * 60;
	private const int AttackDuration = 5 * 60;
	private const int FireInterval = 5;
	private const int GroundSearchTiles = 40;

	private const float DetectionRange = 800f;
	private const float LoseTargetRange = 1200f;
	private const float AttackRange = 480f;
	private const float PatrolMinDistance = 160f;
	private const float PatrolMaxDistance = 480f;
	private const float PatrolArrivalDistance = 10f;
	private const float MinimumGroundClearance = 96f;
	private const float MoveSpeed = 4f;
	private const float MoveResponsiveness = 0.12f;
	private const float AttackBrakeFactor = 0.9f;
	private const float AttackStopSpeed = 0.08f;
	private const float BulletSpeed = 14f;
	private const float SweepHalfAngle = MathHelper.Pi / 6f;
	private const float SweepPeriod = 120f;

	private const int BulletDamage = 14;

	private enum AiState
	{
		Idle,
		Pursue,
		Attack,
		Patrol
	}

	private AiState State {
		get => (AiState)(int)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private float StateTimer {
		get => NPC.ai[1];
		set => NPC.ai[1] = value;
	}

	// ai[2] 用  “玩家索引 + 1”，从而让默认值 0 表示没有目标。
	private int TargetPlayerIndex {
		get => (int)NPC.ai[2] - 1;
		set => NPC.ai[2] = value + 1;
	}

	private float PatrolTargetX {
		get => NPC.ai[3];
		set => NPC.ai[3] = value;
	}

	private int _drawFrame = 1;

	public override string Texture => TextureRoot + "_1";

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 1;
	}

	public override void SetDefaults()
	{
		NPC.width = 64;
		NPC.height = 32;
		NPC.lifeMax = 1500;
		NPC.damage = 33;
		NPC.defense = 8;
		NPC.knockBackResist = 0.35f;
		NPC.value = Item.buyPrice(0, 0, 5, 0);
		NPC.npcSlots = 1f;

		NPC.aiStyle = -1;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.lavaImmune = true;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.DeathSound = SoundID.NPCDeath14;
	}

	public override void OnSpawn(IEntitySource source)
	{
		State = AiState.Idle;
		StateTimer = 0f;
		TargetPlayerIndex = -1;
		PatrolTargetX = NPC.Center.X;
		NPC.direction = 1;
		NPC.spriteDirection = 1;
		NPC.velocity = Vector2.Zero;

		if (!HasAuthority) {
			return;
		}

		RaiseToMinimumGroundClearance();
		NPC.netUpdate = true;
	}

	public override void AI()
	{
		NPC.velocity.Y = 0f;

		switch (State) {
			case AiState.Idle:
				UpdateIdle();
				break;
			case AiState.Pursue:
				UpdatePursuit();
				break;
			case AiState.Attack:
				UpdateAttack();
				break;
			case AiState.Patrol:
				UpdatePatrol();
				break;
			default:
				if (HasAuthority)
					EnterIdle();
				break;
		}

		NPC.rotation = NPC.velocity.X * 0.015f;
		UpdateAnimation();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateIdle()
	{
		NPC.velocity.X *= 0.82f;
		if (System.Math.Abs(NPC.velocity.X) < 0.05f)
			NPC.velocity.X = 0f;

		StateTimer++;

		if (HasAuthority) {
			int scannedPlayer = FindNearestPlayer(DetectionRange);
			if (scannedPlayer != TargetPlayerIndex) {
				TargetPlayerIndex = scannedPlayer;
				NPC.netUpdate = true;
			}

			if (StateTimer >= IdleDuration) {
				if (scannedPlayer >= 0)
					EnterPursuit(scannedPlayer);
				else
					EnterPatrol(ChoosePatrolTargetX());
				return;
			}
		}

		if (TryGetTargetPlayer(out Player target))
			Face(target.Center.X);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePursuit()
	{
		StateTimer++;

		if (!TryGetTargetPlayer(out Player target)) {
			NPC.velocity.X = 0f;
			if (HasAuthority)
				EnterIdle();
			return;
		}

		float horizontalDistance = target.Center.X - NPC.Center.X;
		Face(horizontalDistance + NPC.Center.X);

		if (HasAuthority && System.Math.Abs(horizontalDistance) <= AttackRange) {
			EnterAttack(TargetPlayerIndex);
			return;
		}

		if (HasAuthority && Vector2.DistanceSquared(NPC.Center, target.Center) > LoseTargetRange * LoseTargetRange) {
			EnterIdle();
			return;
		}

		MoveTowardsX(target.Center.X);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateAttack()
	{
		if (!TryGetTargetPlayer(out Player target)) {
			NPC.velocity.X *= AttackBrakeFactor;
			if (HasAuthority)
				EnterIdle();
			return;
		}

		Face(target.Center.X);

		if (System.Math.Abs(NPC.velocity.X) > AttackStopSpeed) {
			NPC.velocity.X *= AttackBrakeFactor;
			return;
		}

		NPC.velocity.X = 0f;
		StateTimer++;

		int timer = (int)StateTimer;
		if (timer % FireInterval == 0) {
			if (!Main.dedServ)
				SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.45f, Pitch = 0.1f, MaxInstances = 4 }, NPC.Center);

			if (HasAuthority)
				FireSweepingShot(target, timer);
		}

		if (HasAuthority && StateTimer >= AttackDuration)
			EnterIdle();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePatrol()
	{
		StateTimer++;
		MoveTowardsX(PatrolTargetX);

		if (HasAuthority && System.Math.Abs(PatrolTargetX - NPC.Center.X) <= PatrolArrivalDistance)
			EnterIdle();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void MoveTowardsX(float targetX)
	{
		float difference = targetX - NPC.Center.X;
		float desiredVelocity = MathHelper.Clamp(difference * 0.08f, -MoveSpeed, MoveSpeed);
		NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, desiredVelocity, MoveResponsiveness);
		Face(targetX);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void FireSweepingShot(Player target, int timer)
	{
		Vector2 muzzlePosition = NPC.Center + new Vector2(27f * NPC.spriteDirection, 14f);
		Vector2 baseDirection = (target.Center - muzzlePosition).SafeNormalize(new Vector2(NPC.spriteDirection, 0f));
		float sweepAngle = (float)System.Math.Sin(MathHelper.TwoPi * timer / SweepPeriod) * SweepHalfAngle;
		Vector2 velocity = baseDirection.RotatedBy(sweepAngle) * BulletSpeed;

		Projectile.NewProjectile(
			NPC.GetSource_FromAI(),
			muzzlePosition,
			velocity,
			ModContent.ProjectileType<RaptorDroneBullet>(),
			BulletDamage,
			1f,
			Main.myPlayer);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int FindNearestPlayer(float range)
	{
		int nearestPlayer = -1;
		float nearestDistanceSquared = range * range;

		for (int i = 0; i < Main.maxPlayers; i++) {
			Player player = Main.player[i];
			if (!player.active || player.dead)
				continue;

			float distanceSquared = Vector2.DistanceSquared(NPC.Center, player.Center);
			if (!(distanceSquared <= nearestDistanceSquared)) {
				continue;
			}

			nearestDistanceSquared = distanceSquared;
			nearestPlayer = i;
		}

		return nearestPlayer;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryGetTargetPlayer(out Player player)
	{
		int index = TargetPlayerIndex;
		if (index >= 0 && index < Main.maxPlayers) {
			Player candidate = Main.player[index];
			if (candidate.active && !candidate.dead) {
				player = candidate;
				return true;
			}
		}

		player = null;
		return false;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float ChoosePatrolTargetX()
	{
		float direction = Main.rand.NextBool() ? 1f : -1f;
		float distance = Main.rand.NextFloat(PatrolMinDistance, PatrolMaxDistance);
		float worldLeft = 10f * 16f;
		float worldRight = Main.maxTilesX * 16f - 10f * 16f;
		float targetX = MathHelper.Clamp(NPC.Center.X + direction * distance, worldLeft, worldRight);

		// 靠近世界边缘时改向，避免刚选完目标就立即抵达。
		if (System.Math.Abs(targetX - NPC.Center.X) < PatrolMinDistance * 0.5f)
			targetX = MathHelper.Clamp(NPC.Center.X - direction * distance, worldLeft, worldRight);

		return targetX;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void RaiseToMinimumGroundClearance()
	{
		int tileX = System.Math.Clamp((int)(NPC.Center.X / 16f), 10, Main.maxTilesX - 10);
		int startTileY = System.Math.Clamp((int)(NPC.Bottom.Y / 16f), 10, Main.maxTilesY - 10);
		int endTileY = System.Math.Min(startTileY + GroundSearchTiles, Main.maxTilesY - 10);

		float minimumWorldY = 10f * 16f;
		for (int tileY = startTileY; tileY <= endTileY; tileY++) {
			Tile tile = Framing.GetTileSafely(tileX, tileY);
			if (!tile.HasTile || tile.IsActuated || (!Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]))
				continue;

			float groundY = tileY * 16f;
			if (groundY - NPC.Bottom.Y >= MinimumGroundClearance)
				return;

			float raisedTop = groundY - MinimumGroundClearance - NPC.height;
			while (raisedTop > minimumWorldY && Collision.SolidCollision(new Vector2(NPC.position.X, raisedTop), NPC.width, NPC.height))
				raisedTop -= 16f;

			NPC.position.Y = System.Math.Max(raisedTop, minimumWorldY);
			return;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Face(float targetX)
	{
		float difference = targetX - NPC.Center.X;
		if (System.Math.Abs(difference) < 1f)
			return;

		NPC.direction = difference > 0f ? 1 : -1;
		NPC.spriteDirection = NPC.direction;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnterIdle()
	{
		State = AiState.Idle;
		StateTimer = 0f;
		TargetPlayerIndex = -1;
		NPC.netUpdate = true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnterPursuit(int playerIndex)
	{
		State = AiState.Pursue;
		StateTimer = 0f;
		TargetPlayerIndex = playerIndex;
		NPC.netUpdate = true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnterAttack(int playerIndex)
	{
		State = AiState.Attack;
		StateTimer = 0f;
		TargetPlayerIndex = playerIndex;
		NPC.netUpdate = true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnterPatrol(float targetX)
	{
		State = AiState.Patrol;
		StateTimer = 0f;
		TargetPlayerIndex = -1;
		PatrolTargetX = targetX;
		NPC.netUpdate = true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateAnimation()
	{
		int timer = (int)StateTimer;
		if (State != AiState.Attack) {
			_drawFrame = 1 + timer / 5 % 5;
			return;
		}

		bool muzzleFlash = timer >= FireInterval && timer % FireInterval <= 1;
		_drawFrame = muzzleFlash ? 12 : 13;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{TextureRoot}_{_drawFrame}").Value;
		SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		Main.EntitySpriteDraw(
			texture,
			NPC.Center - screenPos,
			null,
			NPC.GetAlpha(drawColor),
			NPC.rotation,
			texture.Size() * 0.5f,
			NPC.scale,
			effects);

		return false;
	}

	private static bool HasAuthority => Main.netMode != NetmodeID.MultiplayerClient;
}

public class RaptorDroneBullet : ModProjectile
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Bullet}";

	public override void SetDefaults()
	{
		Projectile.width = 6;
		Projectile.height = 6;
		Projectile.aiStyle = 0;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.penetrate = 1;
		Projectile.timeLeft = 180;
		Projectile.tileCollide = true;
		Projectile.ignoreWater = false;
		Projectile.extraUpdates = 2;
	}

	public override void AI()
	{
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.25f, 0.08f));
	}
}
