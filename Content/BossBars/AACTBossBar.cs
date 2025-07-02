using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace ArknightsMod.Content.BossBars
{
	public class AACTBossBar : ModBossBar
	{
		private int bossHeadIndex = -1;

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return bossHeadIndex != -1 ? TextureAssets.NpcHeadBoss[bossHeadIndex] : null;
		}

		private float timer1 = 0;
		private float timer2 = 0;
		private float timer3 = 0;
		private float randomtimer = 0;
		private float randomshake;
		private float stage3health;

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active) {
				return false;
			}
			stage3health = npc.lifeMax * 0.45f;
			bossHeadIndex = npc.GetBossHeadTextureIndex();

			if (npc.ai[0] < 2) {
				timer1++;
				if (timer1 > 120) {
					timer1 = 120;
				}
				shield = (float)Math.Sin(timer1 * Math.PI / 240) * (npc.life - npc.lifeMax * 0.9f);
				shieldMax = npc.lifeMax * 0.1f;
				life = 0;
				lifeMax = 0;
			}
			else if (npc.ai[0] >= 2 && npc.ai[0] < 3) {
				timer2++;
				if (timer2 > 120) {
					timer2 = 120;
				}
				life = (float)Math.Sin(timer2 * Math.PI / 240) * (npc.life - npc.lifeMax * 0.45f);
				lifeMax = npc.lifeMax * 0.45f;
			}
			else if (npc.ai[0] == 3) {
				timer3++;
				if (timer3 > 120) {
					timer3 = 120;
				}

				randomtimer++;
				randomshake = (float)(-4 * Math.Sin(randomtimer / 12) - 2 * Math.Sin(randomtimer / 4) - 3 * Math.Sin(randomtimer / 2) + 4 * Math.Sin(randomtimer / 144) + 6 * Math.Cos(randomtimer / 48) - 4 * Math.Cos(randomtimer / 24));

				life = (float)Math.Sin(timer3 * Math.PI / 240) * npc.life + (npc.lifeMax * 0.9f - npc.life) / stage3health * npc.lifeMax * 0.1f * randomshake / 20;
				lifeMax = stage3health - (npc.lifeMax * 0.9f - npc.life) / stage3health * npc.lifeMax * 0.05f * randomshake / 20;

				if (life > lifeMax) {
					life = lifeMax;
				}

				if (life < 0) {
					life = 0;
				}

				if (lifeMax < 0) {
					lifeMax = 0;
				}
			}

			return npc.ai[1] == 0 ? false : true;
		}
	}
}