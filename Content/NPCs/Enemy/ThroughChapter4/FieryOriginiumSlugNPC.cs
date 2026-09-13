using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class FieryOriginiumSlugNPC : ModNPC
	{
		private enum ActionState { Walk, Attack }

		private ActionState _state = ActionState.Walk;
		private int _attackTimer;
		private int _nextAttackCooldown;
		private int _status;

		private const float DetectRange = 480f;
		private const float AttackRange = 360f;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 10;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Velocity = 1f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults()
		{
			NPC.width = 46;
			NPC.height = 36;
			NPC.defense = 8;
			NPC.lifeMax = 180;
			NPC.knockBackResist = 0.5f;
			NPC.value = 50f;
			NPC.aiStyle = NPCAIStyleID.Snail;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			if (Main.masterMode)
				NPC.damage = 12;
			else
				NPC.damage = 4;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.Add(
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.FieryOriginiumSlug")));
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.spriteDirection = NPC.direction;

			if (_state == ActionState.Walk)
			{
				const int startFrame = 0, endFrame = 5, frameSpeed = 6;
				if (NPC.velocity.Length() > 0.01f)
					NPC.frameCounter += 0.6f + NPC.velocity.Length() / 4f;
				if (NPC.frameCounter > frameSpeed)
				{
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;
					if (NPC.frame.Y < startFrame * frameHeight || NPC.frame.Y > endFrame * frameHeight)
						NPC.frame.Y = startFrame * frameHeight;
				}
			}
			else
			{
				const int startFrame = 6, endFrame = 9, frameSpeed = 5;
				if (NPC.frame.Y < startFrame * frameHeight || NPC.frame.Y > endFrame * frameHeight)
					NPC.frame.Y = startFrame * frameHeight;
				NPC.frameCounter += 1f;
				if (NPC.frameCounter > frameSpeed)
				{
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;
					if (NPC.frame.Y > endFrame * frameHeight)
						NPC.frame.Y = startFrame * frameHeight;
				}
			}
		}

		public override void AI()
		{
			if (_nextAttackCooldown <= 0)
				_nextAttackCooldown = Main.rand.Next(20, 61);

			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
				NPC.TargetClosest();

			if (!NPC.HasValidTarget) { RandomWalk(); return; }

			Player target = Main.player[NPC.target];
			float distance = Vector2.Distance(NPC.Center, target.Center);

			if (distance > DetectRange) { RandomWalk(); return; }

			if (_state == ActionState.Walk)
				Approach(target, distance);
			else
				Attack(target, distance);
		}

		private void RandomWalk()
		{
			_state = ActionState.Walk;
			NPC.ai[3]++;
			if (NPC.ai[3] % 180 == 0)
			{
				_status = Main.rand.Next(5);
				NPC.direction = Main.rand.NextBool() ? 1 : -1;
			}
			NPC.velocity.X = 0.8f * NPC.direction;
			if (NPC.collideX) NPC.velocity.Y = 1.2f * NPC.directionY;
		}

		private void Approach(Player target, float distance)
		{
			if (Math.Abs(target.Center.X - NPC.Center.X) > 2f)
			{
				NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
				NPC.velocity.X = 1f * NPC.direction;
			}
			else NPC.velocity.X = 0f;

			if (NPC.collideX) NPC.velocity.Y = 1.2f * NPC.directionY;

			_attackTimer++;
			if (distance <= AttackRange && _attackTimer >= _nextAttackCooldown)
			{
				_state = ActionState.Attack;
				_attackTimer = 0;
				NPC.netUpdate = true;
			}
		}

		private void Attack(Player target, float distance)
		{
			NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
			NPC.velocity *= 0f;
			NPC.ai[3]++;

			if (NPC.ai[3] == 15 && distance <= AttackRange && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 velocity = CalculateArcVelocity(target.Center, NPC.Center, 8.5f, 0.22f);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
					ProjectileType<FierySlugFireballProjectile>(), 10, 0f, Main.myPlayer);
			}

			if (NPC.ai[3] >= 35)
			{
				NPC.ai[3] = 0;
				_state = ActionState.Walk;
				_nextAttackCooldown = Main.rand.Next(20, 61);
				NPC.netUpdate = true;
			}
		}

		private static Vector2 CalculateArcVelocity(Vector2 target, Vector2 origin, float speedBase, float gravity)
		{
			Vector2 toTarget = target - origin;
			float distance = toTarget.Length();
			float flightTicks = Math.Clamp(distance / speedBase, 18f, 48f);
			Vector2 velocity = toTarget / flightTicks;
			velocity.Y -= gravity * flightTicks * 0.5f;
			return velocity;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch);
				dust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
				dust.scale *= 1.2f;
				dust.noGravity = true;
			}
		}
	}
}
