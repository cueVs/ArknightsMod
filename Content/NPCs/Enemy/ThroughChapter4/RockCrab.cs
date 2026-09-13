using ArknightsMod.Content.Items.Material;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class RockCrab : ModNPC
	{
		private const int FrameHeight = 46;
		private const int AttackStartFrame = 0;
		private const int AttackEndFrame = 4;
		private const int IdleStartFrame = 5;
		private const int IdleEndFrame = 12;
		private const int WalkStartFrame = 12;
		private const int WalkEndFrame = 18;
		private const int FrameSpeed = 6;
		private const float MaxStepJumpSpeed = 3.5f;
		private const float AttackAnimDuration = (AttackEndFrame - AttackStartFrame + 1) * FrameSpeed;
		private const int ContactHitCooldown = 50;
		private const int ContactPlayerImmunity = 24;
		private const int AttackPlayerImmunity = 36;
		private const int AttackHitFrame = 2;
		private const float AttackHorizontalRange = 16f * 2f;
		private const float AttackVerticalRange = 34f;
		private const int AttackHitRemaining = (int)AttackAnimDuration - AttackHitFrame * FrameSpeed;

		private int contactHitCooldown;
		private bool attackDamageDealt;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 19;

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			NPC.width = 70;
			NPC.height = FrameHeight;
			NPC.damage = 22;
			NPC.defense = 10;
			NPC.lifeMax = 90;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 120f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AnimationType = -1;

			AIType = NPCID.CorruptBunny;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY <= Main.rockLayer || spawnInfo.SpawnTileY >= Main.maxTilesY - 210)
				return 0f;

			float chance = SpawnCondition.BoundCaveNPC.Chance;
			if (chance <= 0f)
				return 0f;

			return chance * 0.4f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<OrironShard>(), 1, 8, 8));
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.RockCrab")),
			});
		}

		public override void AI() {
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			if (NPC.localAI[3] > 0f)
				NPC.localAI[3]--;

			if (contactHitCooldown > 0)
				contactHitCooldown--;

			bool attacking = NPC.localAI[0] > 0f;
			if (attacking) {
				NPC.localAI[0]--;
				if (NPC.localAI[0] <= 0f)
					NPC.localAI[3] = 45f;
			}
			else if (NPC.localAI[3] <= 0f && IsInAttackRange(target)) {
				NPC.localAI[0] = AttackAnimDuration;
				NPC.localAI[2] = GetAttackSpriteDirection(target);
				attackDamageDealt = false;
				NPC.ai[1] = 0f;
				NPC.ai[2] = 0f;
				NPC.ai[3] = 0f;
			}

			if (NPC.localAI[0] > 0f) {
				NPC.ai[1] = 0f;
				NPC.ai[2] = 0f;
				NPC.velocity.X = 0f;
				if (NPC.velocity.Y < 0f)
					NPC.velocity.Y = 0f;
				NPC.spriteDirection = (int)NPC.localAI[2];

				if (!attackDamageDealt && NPC.localAI[0] == AttackHitRemaining)
					TryDealAttackDamage(target);

				return;
			}

			// 腐化兔式 Fighter AI 会大跳；限制为仅能越过 1~2 格台阶的小跳
			if (NPC.velocity.Y < -MaxStepJumpSpeed)
				NPC.velocity.Y = -MaxStepJumpSpeed;

			if (NPC.collideX && NPC.velocity.Y == 0f && Math.Abs(NPC.velocity.X) > 0.05f) {
				int stepDirection = Math.Sign(NPC.velocity.X);
				if (stepDirection == 0)
					stepDirection = NPC.direction;

				if (CanStepUp(stepDirection))
					NPC.velocity.Y = -MaxStepJumpSpeed;
			}
		}

		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			bool attacking = NPC.localAI[0] > 0f;
			int startFrame;
			int endFrame;

			if (attacking) {
				startFrame = AttackStartFrame;
				endFrame = AttackEndFrame;
				NPC.spriteDirection = (int)NPC.localAI[2];
			}
			else if (Math.Abs(NPC.velocity.X) > 0.15f) {
				startFrame = WalkStartFrame;
				endFrame = WalkEndFrame;
				NPC.spriteDirection = NPC.velocity.X > 0f ? 1 : -1;
			}
			else {
				startFrame = IdleStartFrame;
				endFrame = IdleEndFrame;
				NPC.spriteDirection = NPC.direction;
			}

			int previousState = (int)NPC.localAI[1];
			int currentState = attacking ? 0 : (Math.Abs(NPC.velocity.X) > 0.15f ? 1 : 2);
			if (previousState != currentState) {
				NPC.frame.Y = startFrame * frameHeight;
				NPC.frameCounter = 0;
				NPC.localAI[1] = currentState;
			}

			NPC.frameCounter++;
			if (NPC.frameCounter >= FrameSpeed) {
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;

				if (NPC.frame.Y > endFrame * frameHeight)
					NPC.frame.Y = startFrame * frameHeight;
			}
		}

		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 8; i++) {
				int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, hit.HitDirection, -1f, 0, default, 1f);
				Main.dust[dust].noGravity = true;
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			if (NPC.localAI[0] > 0f)
				return false;

			return contactHitCooldown <= 0;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			contactHitCooldown = ContactHitCooldown;
			target.immuneTime = Math.Max(target.immuneTime, ContactPlayerImmunity);
		}

		private int GetAttackSpriteDirection(Player target) {
			// 攻击动画素材默认朝右，按相对玩家位置取反后翻转
			return target.Center.X >= NPC.Center.X ? -1 : 1;
		}

		private bool IsInActiveAttackRange(Player target) {
			if (target.dead || !target.active)
				return false;

			return Math.Abs(target.Center.X - NPC.Center.X) <= AttackHorizontalRange
				&& Math.Abs(target.Center.Y - NPC.Center.Y) <= AttackVerticalRange;
		}

		private void TryDealAttackDamage(Player target) {
			if (!IsInActiveAttackRange(target))
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int hitDirection = target.Center.X >= NPC.Center.X ? 1 : -1;
			target.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), NPC.damage, hitDirection);

			attackDamageDealt = true;
			contactHitCooldown = ContactHitCooldown;

			if (Main.myPlayer == target.whoAmI)
				target.immuneTime = Math.Max(target.immuneTime, AttackPlayerImmunity);
		}

		private bool IsInAttackRange(Player target) {
			if (target.dead || !target.active)
				return false;

			return Math.Abs(target.Center.X - NPC.Center.X) < 52f
				&& Math.Abs(target.Center.Y - NPC.Center.Y) < 34f;
		}

		private bool CanStepUp(int direction) {
			int tileX = (int)(NPC.Center.X / 16f) + direction;
			int tileY = (int)((NPC.position.Y + NPC.height) / 16f);

			for (int height = 1; height <= 2; height++) {
				if (WorldGen.SolidTile(tileX, tileY - height) && !WorldGen.SolidTile(tileX, tileY - height - 1))
					return true;
			}

			return false;
		}
	}
}
