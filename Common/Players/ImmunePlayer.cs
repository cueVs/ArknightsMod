using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Players
{
	
	public class ImmunePlayer:ModPlayer
	{
		public float ImmuneMultiplier = 1; // 免疫倍数，默认为1
										 // 免疫玩家类，用于处理玩家的免疫状态
										 // 可以在这里添加属性和方法来管理玩家的免疫状态
		public override void PostHurt(Player.HurtInfo info) {
			Player.immuneTime = (int)(Player.immuneTime * ImmuneMultiplier);// 乘以免疫倍数
			ImmuneMultiplier = 1; // 重置免疫倍数
		}
		public bool IsImmune { get; set; } = false; // 是否免疫状态                                                                                                                                                                                                                                                                                                                                                                                                                 
		public override void ResetEffects() // 重置效果方法
		{
			IsImmune = false; // 每次重置时将免疫状态设置为false
		}
	}
	
	
}
