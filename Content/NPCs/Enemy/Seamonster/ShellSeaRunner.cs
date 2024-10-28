using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Terraria.GameContent.Biomes.Desert;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
    public class ShellSeaRunner: ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 10;
           


        }
        public override void FindFrame(int frameHeight)

        {
            Player p = Main.player[NPC.target];
            int Startframe = 1;
            int Endframe = 4;
            int Framespeed = 5;
            if (NPC.ai[3] < 200 || NPC.frame.Y > (Endframe + 1 ) * frameHeight)
            {
                NPC.frameCounter++;
            }
            if (NPC.frame.Y == Endframe * frameHeight && NPC.frame.Y < (Endframe + 1 )* frameHeight)
            {
                NPC.frame.Y = Startframe * frameHeight;
            }
            if (NPC.frame.Y >= 9 * frameHeight)
            {
                NPC.frame.Y = 1 * frameHeight;
            }
            if (NPC.frameCounter > Framespeed)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            if (NPC.position.X - p.position.X < 60 && NPC.position.X - p.position.X >= -60 && NPC.position.Y - p.position.Y < 80 && NPC.position.Y - p.position.Y > -80)
            {
                if (NPC.frame.Y < (Endframe + 1) * frameHeight)
                {
                    NPC.frame.Y = (Endframe + 1 )* frameHeight;

                }

            }
        }
        public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 30;
            NPC.damage = 24;
            NPC.defense = 0;
            NPC.lifeMax = 135;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 1000;
            NPC.knockBackResist = 0.9f;
            NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
            AnimationType = -1; 
            NPC.npcSlots = 2;
            
            NPC.friendly = false;
            NPC.noGravity = false;

        }
        public override void AI()
        {

            NPC.ai[3]++;
            NPC.ai[2]++;
            NPC.ai[1]++;
            NPC.TargetClosest(true);
            Player p = Main.player[NPC.target];
            float acceleration = 0.04f;
            float maxSpeed = 5f;
            if (NPC.ai[3] <= 200)
            {
                
                if (NPC.Center.X > (p.Center.X+10))
                {
                    NPC.velocity.X -= acceleration;
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X -= acceleration;
                    if (NPC.velocity.X < -maxSpeed)
                        NPC.velocity.X = -maxSpeed;
                }
                if (NPC.Center.X <= (p.Center.X-10))
                {
                    NPC.velocity.X += acceleration;
                    if (NPC.velocity.X < 0)
                        NPC.velocity.X += acceleration;
                    if (NPC.velocity.X > maxSpeed)
                        NPC.velocity.X = maxSpeed;
                }
                if (NPC.collideX == true && NPC.ai[2] > 100)
                {
                    NPC.velocity.Y = -7f;
                    NPC.ai[2] = 0;
                }
                if ((NPC.position.Y - p.position.Y > 80|| HoleBelow()) && NPC.ai[2] > 100)
                {
                    NPC.velocity.Y = -7f;
                    NPC.ai[2] = 0;
                }
            }

            if (NPC.ai[3] == 200)
            {
                NPC.ai[3] = 0;
            }
            if (NPC.ai[3] < 300 && NPC.ai[3] > 200)
            {

                NPC.velocity.X = 0;

            }
            if (NPC.ai[3] >= 300)
            {
                NPC.ai[3] = 0;
                NPC.velocity = 5 * (p.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            }
            NPC.direction = NPC.Center.X > p.Center.X ? 0 : 1;
            NPC.spriteDirection = NPC.direction;
        }
        private bool HoleBelow()
        {
            //width of npc in tiles
            int tileWidth = 4;
            int tileX = (int)(NPC.Center.X / 16f) - tileWidth;
            if (NPC.velocity.X > 0) //if moving right
            {
                tileX += tileWidth;
            }
            int tileY = (int)((NPC.position.Y + NPC.height) / 16f);
            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + tileWidth; x++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return false;
                    }
                }
            }
            return true;

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

            //This makes it 5 seconds, one second is 60 ticks

            NPC.ai[3] = 202;

        }
        public override void OnKill()
        {
            int Gore1 = Mod.Find<ModGore>("ShellSeaRunner1").Type;
            var entitySource = NPC.GetSource_Death();
            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("ShellSeaRunner2").Type);
            }
            Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("ShellSeaRunner3").Type);
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("ShellSeaRunner4").Type);
		}
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TransmutedSalt>(), 8, 1, 2));

		}
    }
}
