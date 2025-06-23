using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.Evolution
{
	public class Evolution: ModNPC {
		//转阶段参数
		private bool Stage1 = true;
		private bool Stage2 = false;
		private bool Stage3 = false;
		private int Stage1Time = 0;
		private int Stage2Time = 0;
		private int Stage3Time = 0;
		private int Stage1MaxTime = 60; // 一分钟进化
		private int Stage2MaxTime = 60; // 一分钟进化
		private int Stage2ImmuneTime = 10; // 10秒免疫伤害
		private int Stage3ImmuneTime = 10; // 10秒免疫伤害
		//数值参数
		private int defense = 50;
		private int SpellResist = 0; // 法术抗性(填明日方舟里的法抗）
		private int Health = 5000;
		private int AttackDamage1 = 70;//接触伤害
		private int AttackDamage2 = 60;//射弹伤害
		private int AttackDamage3 = 80;//真实伤害
		//动画参数
		private int frameNumber = 30; //一共多少帧
		private int Stage1Frame = 7; //第一阶段最后一帧(-1)
		private int Change12Frame = 0; //一转二阶段最后一帧(-1)
		private int Stage2Frame = 14; //第二阶段最后一帧(-1)
		private int Change23Frame = 0; //二转三阶段最后一帧(-1)
		private int Stage3Frame = 15;//三阶段最后一帧(-1)
		



	}
}
