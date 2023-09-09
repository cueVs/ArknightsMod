using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
//using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.GameContent.UI.BigProgressBar;

namespace ArknightsMod.Content.BossBars
{
	public class IACTBossBarMT : ModBossBar
	{
		private int bossHeadIndex = -1;

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
		{
			if (bossHeadIndex != -1)
			{
				return TextureAssets.NpcHeadBoss[bossHeadIndex];
			}
			return null;
		}

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
		{
			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active)
			{
				return false;
			}
			bossHeadIndex = npc.GetBossHeadTextureIndex();
			life = npc.life;
			lifeMax = npc.lifeMax;
			return true;
		}
	}
}