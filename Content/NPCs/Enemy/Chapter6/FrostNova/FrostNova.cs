using ArknightsMod.Content.Items.Material;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using ArknightsMod.Content.Dusts;
using ArknightsMod.Content.Projectiles.Bosses.FrostNova;
using Microsoft.CodeAnalysis.Operations;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using Terraria.ModLoader.Utilities;

namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	// The main part of the boss, usually referred to as "body"
	[AutoloadBossHead] // This attribute looks for a texture called "ClassName_Head_Boss" and automatically registers it as the NPC boss head icon
	public class FrostNova : ModNPC
	{
		// This code here is called a property: It acts like a variable, but can modify other things. In this case it uses the NPC.ai[] array that has four entries.
		// We use properties because it makes code more readable ("if (SecondStage)" vs "if (NPC.ai[0] == 1f)").
		// We use NPC.ai[] because in combination with NPC.netUpdate we can make it multiNPC compatible. Otherwise (making our own fields) we would have to write extra code to make it work (not covered here)
		public bool SecondStage {
			get => NPC.ai[0] == 1f;
			set => NPC.ai[0] = value ? 1f : 0f;
		}
		// If your boss has more than two stages, and since this is a boolean and can only be two things (true, false), consider using an integer or enum

		private enum ActionState : int
		{
			Jump,
			Walk,
			FastWalk,
			SmallAttack,
			LargeAttack,
			Sing,
			Revival,
			Dying
		};

		private ActionState State {
			get { return (ActionState)(int)NPC.ai[1]; }
			set { NPC.ai[1] = (int)value; }
		}

		// More advanced usage of a property, used to wrap around to floats to act as a Vector2
		public Vector2 FrostNovaPosition {
			get => new Vector2(NPC.ai[2], NPC.ai[3]);
			set {
				NPC.ai[2] = value.X;
				NPC.ai[3] = value.Y;
			}
		}

		public ref float DeathTimer => ref NPC.localAI[0];
		public ref float JumpTimer => ref NPC.localAI[1];

		public ref float StateTimer => ref NPC.localAI[2];

		public bool Choice;
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.ZoneOverworldHeight && !NPC.AnyNPCs(NPCType<FrostNova>()) ? 0f : 0f;


		// Do NOT try to use NPC.ai[4]/NPC.localAI[4] or higher indexes, it only accepts 0, 1, 2 and 3!
		// If you choose to go the route of "wrapping properties" for NPC.ai[], make sure they don't overlap (two properties using the same variable in different ways), and that you don't accidently use NPC.ai[] directly
		//public override float SpawnChance(NPCSpawnInfo spawnInfo) {
		//	return SpawnCondition.Overworld.Chance;
		//	// return SpawnCondition.OverworldNightMonster.Chance * 1f; // Spawn with 1/5th the chance of a regular zombie.
		//}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 76;

			// Add this in for bosses that have a summon item, requires corresponding code in the item (See MinionBossSummonItem.cs)
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			// Automatically group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			// Specify the debuffs it is immune to. Most NPCs are immune to Confused.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			// This boss also becomes immune to OnFire and all buffs that inherit OnFire immunity during the second half of the fight. See the ApplySecondStageBuffImmunities method.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				CustomTexturePath = "ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FrostNova_preview",
				PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
				PortraitPositionYOverride = 0f,
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			NPC.width = 40;
			NPC.height = 58;
			NPC.damage = 12;
			NPC.defense = 10;
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.DoubleJump;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.value = Item.buyPrice(gold: 5);
			NPC.SpawnWithHigherTime(30);
			NPC.boss = true;
			NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight

			// Default buff immunities should be set in SetStaticDefaults through the NPCID.Sets.ImmuneTo{X} arrays.
			// To dynamically adjust immunities of an active NPC, NPC.buffImmune[] can be changed in AI: NPC.buffImmune[BuffID.OnFire] = true;
			// This approach, however, will not preserve buff immunities. To preserve buff immunities, use the NPC.BecomeImmuneTo and NPC.ClearImmuneToBuffs methods instead, as shown in the ApplySecondStageBuffImmunities method below.

			// Custom AI, 0 is "bound town NPC" AI which slows the NPC down and changes sprite orientation towards the target
			NPC.aiStyle = -1;

			// Custom boss bar
			//NPC.BossBar = ModContent.GetInstance<MinionBossBossBar>();

			// The following code assigns a music track to the boss in a simple way.
			if (!Main.dedServ) {
				Music = MusicLoader.GetMusicSlot(Mod, "Music/Lullabye");
			}
			State = ActionState.Walk;
			JumpTimer = 0;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// Sets the description of this NPC that is listed in the bestiary
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.FrostNova"))
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<IncandescentAlloyBlock>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ItemType<CrystallineCircuit>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ItemType<OptimizedDevice>(), 3, 3, 5));
		}

		public override void OnKill() {
			// This sets downedMinionBoss to true, and if it was false before, it initiates a lantern night
			//NPC.SetEventFlagCleared(ref DownedBossSystem.downedMinionBoss, -1);

			// Since this hook is only ran in singleplayer and serverside, we would have to sync it manually.
			// Thankfully, vanilla sends the MessageID.WorldData packet if a BOSS was killed automatically, shortly after this hook is ran

			// If your NPC is not a boss and you need to sync the world (which includes ModSystem, check DownedBossSystem), use this code:
			/*
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.WorldData);
			}
			*/
		}

		public override void BossLoot(ref string name, ref int potionType) {
			// Here you'd want to change the potion type that drops when the boss is defeated. Because this boss is early pre-hardmode, we keep it unchanged
			// (Lesser Healing Potion). If you wanted to change it, simply write "potionType = ItemID.HealingPotion;" or any other potion type
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
			return true;
		}

		public override bool CheckDead() {
			if (State != ActionState.Dying && SecondStage) //If the boss is defeated, but the death animation hasn't played yet, play the death animation.
			{
				State = ActionState.Dying; //Flag boss as "dying"
				DeathTimer = 0;
				NPC.damage = 0; //Disable contact damage
				NPC.life = 1;
				NPC.dontTakeDamage = true; //Invulnerable
				NPC.netUpdate = true; //Sync to clients
				return false; //Boss isn't dead yet!
			}
			if (State != ActionState.Revival && !SecondStage) {
				DeathTimer = 0;
				NPC.life = 1;
				Choice = true;
				State = ActionState.Revival;
				SecondStage = true;
				NPC.dontTakeDamage = true;
				NPC.netUpdate = true;
				return false;
			}
			return true;
		}

		private void SwitchTo(ActionState state) {
			State = state;
			NPC.netUpdate = true;
		}

		public override void FindFrame(int frameHeight) {
			// This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			// In this case: First stage: 0-1-2-0-1-2, Second stage: 3-4-5-3-4-5, 5 being "total frame count - 1"
			int startFrame;
			int finalFrame;
			int frameSpeed;
			NPC.spriteDirection = NPC.direction;

			if (State == ActionState.Walk || State == ActionState.FastWalk || State == ActionState.Jump) {
				startFrame = 0;
				finalFrame = 4;

				frameSpeed = 5;
				if (State == ActionState.FastWalk) {
					frameSpeed = 4;
				}
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = startFrame * frameHeight;
					}
				}
			}

			if (State == ActionState.Dying) {
				startFrame = 34;
				finalFrame = 38;
				if (NPC.frame.Y < startFrame * frameHeight) {
					// If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
					NPC.frame.Y = startFrame * frameHeight;
				}

				frameSpeed = 10;
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = finalFrame * frameHeight;
					}
				}
			}

			if (State == ActionState.Revival) {
				startFrame = 34;
				finalFrame = 54;
				if (NPC.frame.Y < startFrame * frameHeight) {
					// If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
					NPC.frame.Y = startFrame * frameHeight;
				}

				frameSpeed = 15;
				NPC.frameCounter += 1f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = finalFrame * frameHeight;
						SwitchTo(ActionState.Walk);
						NPC.dontTakeDamage = true;
					}
				}
			}
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// If the NPC dies, spawn gore and play a sound
			if (Main.netMode == NetmodeID.Server) {
				// We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
				return;
			}

			if (NPC.life <= 0) {
			}
		}

		public override void AI() {
			// This should almost always be the first code in AI() as it is responsible for finding the proper player target
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}

			Player player = Main.player[NPC.target];

			if (player.dead) {
				// If the targeted player is dead, flee
				NPC.velocity.Y -= 0.04f;
				// This method makes it so when the boss is in "despawn range" (outside of the screen), it despawns in 10 ticks
				NPC.EncourageDespawn(10);
				return;
			}

			if (State == ActionState.Dying) {
				NPC.velocity.X = 0f;
				Dying();
				return;
			}

			// Be invulnerable during the first stage
			//NPC.dontTakeDamage = !SecondStage;

			if (SecondStage) {
				DoSecondStage(player);
			}
			else {
				DoFirstStage(player);
			}
		}


		private void DoFirstStage(Player player) {
			switch (State) {
				case ActionState.Walk:
					Walk();
					break;
				case ActionState.FastWalk:
					FastWalk();
					break;
				case ActionState.Jump:
					Jump();
					break;
				default:
					Walk();
					break;
			}
		}

		private void DoSecondStage(Player player) {
			if (State == ActionState.Revival) {
				Revival();
				return;
			}
			if (DeathTimer > 0) {
				if (Choice == true) {
					float positionX = NPC.Center.X - 4 * NPC.direction;
					float positionY = NPC.Center.Y - 7;
					Vector2 position = new Vector2(positionX, positionY);
					Projectile.NewProjectile(null, position, Vector2.Zero, ProjectileType<FrostNovaYellowAura>(), 0, 0f, -1, NPC.direction);
					Choice = false;
				}
				NPC.life = NPC.lifeMax;
				NPC.dontTakeDamage = true;
				DeathTimer++;
				if (DeathTimer > 20 * 60) { // TODO
					NPC.dontTakeDamage = false;
					DeathTimer = 0;
				}
			}
			switch (State) {
				case ActionState.Walk:
					Walk();
					break;
				case ActionState.FastWalk:
					FastWalk();
					break;
				case ActionState.Jump:
					Jump();
					break;
				default:
					Walk();
					break;
			}
		}

		private void Revival() {
			NPC.velocity.X = 0f;
			DeathTimer++;
			if (DeathTimer > 2 * 60) {
				NPC.life++;
				if (NPC.life > NPC.lifeMax) {
					NPC.life = NPC.lifeMax;
				}
			}
			if (DeathTimer == 160) {
				float positionX = NPC.Center.X - 4 * NPC.direction;
				float positionY = NPC.Center.Y - 7;
				Vector2 position = new Vector2(positionX, positionY);
				Projectile.NewProjectile(null, position, Vector2.Zero, ProjectileType<FrostNovaRevival>(), 0, 0f, -1, NPC.direction);
			}
			if (DeathTimer == 246) {
				float positionX = NPC.Center.X - 4 * NPC.direction;
				float positionY = NPC.Center.Y - 4;
				Vector2 position = new Vector2(positionX, positionY);
				Projectile.NewProjectile(null, position, Vector2.Zero, ProjectileType<FrostNovaWhiteRing>(), 0, 0f, -1, NPC.direction);
			}
		}


		private void Dying() {
			DeathTimer++;

			if (DeathTimer == 130f) {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					//Vector2 po = NPC.Center + new Vector2(Main.rand.Next(-60, 0), Main.rand.Next(-30, 10));
					//float positionX = NPC.Center.X - 10;
					//float positionY = NPC.Center.Y + 0;
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-20, 23) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 45);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(0.5f * NPC.direction, -0.3f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 90, 0);
					}
					for (int i = 0; i < 4; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-34, 16) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(-10, 34);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(2.8f * NPC.direction, -0.5f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 80, -1);
					}
					for (int i = 0; i < 4; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-18, 28) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(-29, 16);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(-3f * NPC.direction, 0.7f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 85, 1);
					}
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-10, 33) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(10, 36);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(-0.9f * NPC.direction, -0.4f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 85, 1);
					}
				}
			}

			if (DeathTimer >= 135f) {
				NPC.life = 0;
				NPC.HitEffect(0, 0);
				NPC.checkDead(); // This will trigger ModNPC.CheckDead the second time, causing the real death.
			}

			return;
		}
		private void Walk() {
			int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			StateTimer++;
			if (NPC.collideX) {
				JumpTimer++;
			}
			else {
				JumpTimer = 0;
			}

			if (JumpTimer > 0 && JumpTimer < 30) {
				NPC.direction = direction;
				NPC.velocity.X = 0.2f * direction;
				NPC.velocity.Y = -1f;
			}
			else if (JumpTimer >= 30) {
				SwitchTo(ActionState.Jump);
				JumpTimer = 0;
			}
			else {
				NPC.direction = direction;
				NPC.velocity.X = 0.2f * direction;
				if (Main.rand.NextBool(100) && DeathTimer <= 0 && StateTimer > 2 * 60) {
					StateTimer = 0;
					State = ActionState.FastWalk;
					NPC.netUpdate = true;
					FastWalk();
				}
			}
		}

		private void FastWalk() {
			int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			StateTimer++;
			if (NPC.collideX) {
				JumpTimer++;
			}
			else {
				JumpTimer = 0;
			}

			if (JumpTimer > 0 && JumpTimer < 30) {
				NPC.direction = direction;
				NPC.velocity.X = 0.3f * direction;
				NPC.velocity.Y = -1f;
			}
			else if (JumpTimer >= 30) {
				SwitchTo(ActionState.Jump);
				JumpTimer = 0;
			}
			else {
				NPC.direction = direction;
				NPC.velocity.X = 0.4f * direction;
				if (Main.rand.NextBool(100) && StateTimer > 2 * 60) {
					StateTimer = 0;
					State = ActionState.Walk;
					NPC.netUpdate = true;
					Walk();
				}
			}
		}
		private void Jump() {
			int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			JumpTimer++;
			if (JumpTimer < 120) {
				NPC.direction = -direction;
				NPC.velocity.X = -0.2f * direction;
			}
			else if (JumpTimer >= 120 && JumpTimer < 140) {
				NPC.direction = direction;
				NPC.velocity.X = 0.5f * direction;
				NPC.velocity.Y = Math.Min(-6f + (JumpTimer - 120f) * 0.1f, - 0.2f);
			}
			else if (!NPC.collideY && JumpTimer < 280) {
				NPC.direction = direction;
				NPC.velocity.X = 0.8f * direction;
				NPC.velocity.Y = 0.5f;
			}
			else {
				SwitchTo(ActionState.Walk);
				JumpTimer = 0;
			}

			if (JumpTimer == 119) {
				for (int i = 0; i < 3; i++) {
					float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
					float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
					Vector2 position = new Vector2(positionX, positionY);
					Projectile.NewProjectile(null, position, new Vector2(1f * NPC.direction, 1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
				}
				for (int i = 0; i < 3; i++) {
					float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
					float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
					Vector2 position = new Vector2(positionX, positionY);
					Projectile.NewProjectile(null, position, new Vector2(-1f * NPC.direction, 1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
				}
				SoundEngine.PlaySound(SoundID.DoubleJump, NPC.Center);
			}
		}
	}
}
