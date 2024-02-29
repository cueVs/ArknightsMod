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
using ArknightsMod.Content.Dusts;

namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	// The main part of the boss, usually referred to as "body"
	[AutoloadBossHead] // This attribute looks for a texture called "ClassName_Head_Boss" and automatically registers it as the NPC boss head icon
	public class FrostNova : ModNPC
	{
		// This code here is called a property: It acts like a variable, but can modify other things. In this case it uses the NPC.ai[] array that has four entries.
		// We use properties because it makes code more readable ("if (SecondStage)" vs "if (NPC.ai[0] == 1f)").
		// We use NPC.ai[] because in combination with NPC.netUpdate we can make it multiplayer compatible. Otherwise (making our own fields) we would have to write extra code to make it work (not covered here)
		public bool SecondStage {
			get => NPC.ai[0] == 1f;
			set => NPC.ai[0] = value ? 1f : 0f;
		}
		// If your boss has more than two stages, and since this is a boolean and can only be two things (true, false), consider using an integer or enum

		private enum ActionState : int
		{
			Walk,
			SmallAttack,
			LargeAttack,
			Dying,
			Revival,
			Sing
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

		public ref float Death_Timer => ref NPC.localAI[0];

		// Do NOT try to use NPC.ai[4]/NPC.localAI[4] or higher indexes, it only accepts 0, 1, 2 and 3!
		// If you choose to go the route of "wrapping properties" for NPC.ai[], make sure they don't overlap (two properties using the same variable in different ways), and that you don't accidently use NPC.ai[] directly

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 75;

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
			NPC.width = 60;
			NPC.height = 62;
			NPC.damage = 12;
			NPC.defense = 10;
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			DrawOffsetY = -1;
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
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// Sets the description of this NPC that is listed in the bestiary
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.FrostNova"))
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IncandescentAlloyBlock>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrystallineCircuit>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OptimizedDevice>(), 3, 3, 5));
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
			if (State != ActionState.Dying) //If the boss is defeated, but the death animation hasn't played yet, play the death animation.
			{
				State = ActionState.Dying; //Flag boss as "dying"
				Death_Timer = 0;
				NPC.damage = 0; //Disable contact damage
				NPC.life = 1;
				NPC.dontTakeDamage = true; //Invulnerable
				NPC.netUpdate = true; //Sync to clients
				return false; //Boss isn't dead yet!
			}
			if (!SecondStage) {
				NPC.life = NPC.lifeMax;
				State = ActionState.Revival;
				SecondStage = true;
				NPC.netUpdate = true;
				return true;
			}
			return true;
		}

		private void SwitchTo(ActionState state) {
			State = state;
		}

		public override void FindFrame(int frameHeight) {
			// This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			// In this case: First stage: 0-1-2-0-1-2, Second stage: 3-4-5-3-4-5, 5 being "total frame count - 1"
			int startFrame = 0;
			int finalFrame = 3;

			if (State == ActionState.Walk) {
				startFrame = 0;
				finalFrame = 3;

				int frameSpeed = 5;
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
				startFrame = 33;
				finalFrame = 41;
				if (NPC.frame.Y < startFrame * frameHeight) {
					// If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
					NPC.frame.Y = startFrame * frameHeight;
				}

				int frameSpeed = 10;
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
				startFrame = 33;
				finalFrame = 53;
				if (NPC.frame.Y < startFrame * frameHeight) {
					// If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
					NPC.frame.Y = startFrame * frameHeight;
				}

				int frameSpeed = 5;
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = finalFrame * frameHeight;
						SwitchTo(ActionState.Walk);
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

		

		//private void CheckSecondStage() {
		//	if (SecondStage) {
		//		// No point checking if the NPC is already in its second stage
		//		return;
		//	}

		//	if (NPC.life <= 0 && Main.netMode != NetmodeID.MultiplayerClient) {
		//		// If we have no shields (aka "no minions alive"), we initiate the second stage, and notify other players that this NPC has reached its second stage
		//		// by setting NPC.netUpdate to true in this tick. It will send important data like position, velocity and the NPC.ai[] array to all connected clients

		//		State = ActionState.Revival;
		//		NPC.life = NPC.lifeMax; //HP set to max
		//		// Because SecondStage is a property using NPC.ai[], it will get synced this way
		//		SecondStage = true;
		//		NPC.netUpdate = true;
		//	}
		//}

		private void DoFirstStage(Player player) {
			State = ActionState.Walk;
		}

		private void DoSecondStage(Player player) {
			if (State != ActionState.Revival) {
				State = ActionState.Walk;
			}


		}


		private void Dying() {
			Death_Timer ++;
			int probability = Math.Max(38 - (int)Math.Round(Death_Timer / 10), 1);
			if (Main.rand.NextBool(probability) && 250 < Death_Timer && Death_Timer < 490f) {
				for (int d = 0; d < 3; d++) {
					Dust dust = Main.dust[Dust.NewDust(NPC.Left, NPC.width, NPC.height / 2, DustID.SilverFlame, 0f, 0f, 0, default(Color), 1f)];
					dust.position = NPC.Bottom + Vector2.UnitX * Main.rand.Next(-20, 20);
					dust.velocity.X = 0f;
					dust.velocity.Y = 0f + Main.rand.Next(-3, 0);
					dust.noGravity = false;
				}
			}
			if (Death_Timer >= 260f) {

				NPC.alpha += 1;
				if (NPC.alpha > 255) {
					NPC.alpha = 255;
				}
			}
			if (Death_Timer >= 470f) {
				NPC.life = 0;
				NPC.HitEffect(0, 0);
				NPC.checkDead(); // This will trigger ModNPC.CheckDead the second time, causing the real death.
			}

			return;
		}
	}
}
