using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.OF.Pmp
{
	public class PmpSlugEggLandNPC : ModNPC
	{
		private const int Phase1WaitTicks = 60;
		private const int Phase2ShakeTicks = 30;
		private const int Phase3PauseTicks = 20;
		private const int Phase4ShakeTicks = 30;
		private const int Phase5SpawnDelay = 30;

		public override string Texture => "ArknightsMod/Content/Projectiles/PmpSlugEggLand";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 20;
			NPC.height = 20;
			NPC.lifeMax = 80;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.knockBackResist = 0.2f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.dontTakeDamageFromHostiles = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 0f;
			NPC.npcSlots = 0.2f;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

		public override void AI()
		{
			NPC.velocity.X *= 0.85f;
			if (Math.Abs(NPC.velocity.Y) < 0.01f)
				NPC.velocity.Y = 0f;

			NPC.ai[0]++;
			int t = (int)NPC.ai[0];
			int phase = (int)NPC.ai[1];

			switch (phase)
			{
				case 0:
					if (t >= Phase1WaitTicks) { NPC.ai[1] = 1; NPC.ai[0] = 0; }
					break;

				case 1:
					DoShakeDust();
					if (t >= Phase2ShakeTicks) { NPC.ai[1] = 2; NPC.ai[0] = 0; }
					break;

				case 2:
					if (t >= Phase3PauseTicks) { NPC.ai[1] = 3; NPC.ai[0] = 0; }
					break;

				case 3:
					DoShakeDust();
					if (t >= Phase4ShakeTicks) { NPC.ai[1] = 4; NPC.ai[0] = 0; }
					break;

				case 4:
					if (t >= Phase5SpawnDelay)
					{
						HatchNow();
					}
					break;
			}
		}

		public override bool CheckDead()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				HatchNow();
			}
			return false;
		}

		private void DoShakeDust()
		{
			if (Main.rand.NextBool(2))
			{
				Vector2 spd = new Vector2(Main.rand.NextFloatDirection() * 1.2f, -Main.rand.NextFloat(0.4f, 1.2f));
				int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, spd.X, spd.Y, 120, default, 0.85f);
				Main.dust[d].noGravity = false;
			}
		}

		private void HatchNow()
		{
			Vector2 hatchPos = NPC.Center;

			for (int i = 0; i < 24; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(3.5f, 3.5f);
				int d = Dust.NewDust(hatchPos, 4, 4, DustID.FlameBurst, spd.X, spd.Y, 60, default, 1.3f);
				Main.dust[d].noGravity = true;
			}
			for (int i = 0; i < 10; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(2f, 2f);
				Dust.NewDust(hatchPos, 4, 4, DustID.Smoke, spd.X, spd.Y, 120, default, 1.0f);
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(4f, 2f);
				spd.Y = -Math.Abs(spd.Y) - 1f;
				int d = Dust.NewDust(hatchPos, 4, 4, DustID.Torch, spd.X, spd.Y, 80, default, 1.5f);
				Main.dust[d].noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1, hatchPos);

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)hatchPos.X, (int)hatchPos.Y, NPCType<ThroughChapter4.FieryOriginiumSlugNPC>());
			}

			NPC.life = 0;
			NPC.active = false;
			NPC.netUpdate = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = TextureAssets.Npc[Type].Value;
			Vector2 shakeOffset = Vector2.Zero;
			int ph = (int)NPC.ai[1];
			if (ph == 1 || ph == 3)
				shakeOffset.X = MathF.Sin(NPC.ai[0] * 1.8f) * 1.2f;

			Vector2 drawPos = NPC.Center - screenPos + shakeOffset;
			spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
			return false;
		}
	}
}
