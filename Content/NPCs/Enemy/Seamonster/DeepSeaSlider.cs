using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using ArknightsMod.Content.Items.Material;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
    public class DeepSeaSlider : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 10;
            
            
        }
        public override void FindFrame(int frameHeight)

        {
			NPC.spriteDirection = NPC.direction;
			Player p = Main.player[NPC.target];
            int Startframe = 1;
            int Endframe = 4;
            int Framespeed = 5;
            if (NPC.ai[3] < 200 || NPC.frame.Y > 5 * frameHeight)
            {
                NPC.frameCounter++;
            }
            if (NPC.frame.Y == 4 * frameHeight && NPC.frame.Y < 5 * frameHeight)
            {
                NPC.frame.Y = Startframe * frameHeight;
            }
            if (NPC.frame.Y >= 9 * frameHeight)
            {
                NPC.frame.Y = Startframe * frameHeight;
            }
            if (NPC.frameCounter > Framespeed)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            if (NPC.position.X - p.position.X < 60 && NPC.position.X - p.position.X >= -60 && NPC.velocity.Y == 0)
            {   if(NPC.frame.Y < 5 * frameHeight)
                {
                    NPC.frame.Y = 5 * frameHeight;

                }
            
                
            }
        }
		//public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		//{
		//public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 origin, float scale,SpriteEffects effects,float layerdepth)
		//{

		//}
		// }
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}

		public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 48;
            NPC.damage = 28;
            NPC.defense = 10;
            NPC.lifeMax = 150;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 1600;
            NPC.knockBackResist = 0.7f;
            NPC.aiStyle = 0; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
            AnimationType = -1; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
            // Makes kills of this NPC go towards dropping the banner it's associated with.
            //new int[1] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
            NPC.npcSlots = 3;
            Main.npcFrameCount[Type] = 10;
            NPC.friendly = false;
            NPC.noGravity = false;
			if (Main.expertMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}
		}
        public override void AI()
        {
            
            NPC.ai[3]++;
            
            NPC.TargetClosest(true);
            Player p = Main.player[NPC.target];
            
            if (NPC.ai[3]<=200)
            {
                if (NPC.position.X - p.position.X > 30)
                {
                    NPC.velocity.X = -2.5f;



					if (NPC.collideX) {
						NPC.velocity.Y = -1.2f;
					}

				}
                if (NPC.position.X - p.position.X <= -30)
                {
                    NPC.velocity.X = 2.5f;


					if (NPC.collideX) {
						NPC.velocity.Y = -1.2f;
					}

				}
                
            }
            if (NPC.ai[3]<400 && NPC.ai[3]>200)
            {
            
                NPC.velocity.X = 0;
                
            }
            if (NPC.ai[3]>=400)
            {
                NPC.ai[3]=0;
                NPC.velocity = 5 * (p.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            }
            int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
            NPC.direction = direction;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.OverworldDayRain.Chance * 0.8f;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            // Here we can make things happen if this NPC hits a player via its hitbox (not projectiles it shoots, this is handled in the projectile code usually)
            // Common use is applying buffs/debuffs:

            //int buffType = ModContent.BuffType<AnimatedBuff>();
            // Alternatively, you can use a vanilla buff: int buffType = BuffID.Slow;

            int timeToAdd = 3 * 60; //This makes it 5 seconds, one second is 60 ticks
            target.AddBuff(31, timeToAdd);
            NPC.ai[3] = 200;
            
        }
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 10; i++) {
				int dustType = DustID.BlueMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoagulatingGel>(), 10, 1, 2));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TransmutedSalt>(), 10, 1, 1));



			// Since Party Zombie is essentially just another variation of Zombie, we'd like to mimic the Zombie drops.
			// To do this, we can either (1) copy the drops from the Zombie directly or (2) just recreate the drops in our code.
			// (1) Copying the drops directly means that if Terraria updates and changes the Zombie drops, your ModNPC will also inherit the changes automatically.
			// (2) Recreating the drops can give you more control if desired but requires consulting the wiki, bestiary, or source code and then writing drop code.

			// (1) This example shows copying the drops directly. For consistency and mod compatibility, we suggest using the smallest positive NPCID when dealing with npcs with many variants and shared drop pools.



			// (2) This example shows recreating the drops. This code is commented out because we are using the previous method instead.
			// npcLoot.Add(ItemDropRule.Common(ItemID.Shackle, 50)); // Drop shackles with a 1 out of 50 chance.
			// npcLoot.Add(ItemDropRule.Common(ItemID.ZombieArm, 250)); // Drop zombie arm with a 1 out of 250 chance.

			// Finally, we can add additional drops. Many Zombie variants have their own unique drops: https://terraria.fandom.com/wiki/Zombie
			//npcLoot.Add(ItemDropRule.Common(ModItem.Thorn, 100)); // 1% chance to drop Confetti
		}
        public override void OnKill()
        {
            int Gore1 = Mod.Find<ModGore>("DeepSeaSlider1").Type;
            var entitySource = NPC.GetSource_Death();
            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("DeepSeaSlider3").Type);
            }
            Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("DeepSeaSlider2").Type);
            Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Gore1);
        }
        
        

        
    }
   
}


