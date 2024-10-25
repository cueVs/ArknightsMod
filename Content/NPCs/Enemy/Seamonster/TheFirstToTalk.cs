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
using log4net.Core;
using System;
using Terraria.Audio;
using System.Reflection.Metadata;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class TheFirstToTalk : ModNPC
	{
		public override void SetDefaults() {
			NPC.width = 60;
			NPC.height = 80;
			NPC.damage = 48;
			NPC.scale = 2f;
			NPC.defense = 10;
			NPC.lifeMax = 5500;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 60000;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1;
			NPC.npcSlots = 5;
			NPC.boss = true;
			Main.npcFrameCount[Type] = 64;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode || Main.masterMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}
			
		}
		private int walkframe = 16;
		private int shootframe = 27;
		private int rushframe = 39;
		private int skillstart = 50;
		private int skillframe = 55;
		private int skillend = 64;
		private bool walk;
		private bool shoot;
		private bool rush;
		private bool skill;
		private int SP;
	

		public override void AI() {
			SP++;
			NPC.TargetClosest(true);
			Player Player = Main.player[NPC.target];
			int directionchoose = Player.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float diffX = Player.Center.X - NPC.Center.X;
			float diffY = Player.Center.Y - NPC.Center.Y;
			float distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));//到玩家的距离（格数）
			float acceleration = 0.02f;
			float maxSpeed = 2f;
			float angle = (float)Math.Atan((Player.Center.Y - NPC.Center.Y) / (Player.Center.X - NPC.Center.X));
			Music =MusicLoader.GetMusicSlot("ArknightsMod/Music/TFTT");
		}

	}
	

}