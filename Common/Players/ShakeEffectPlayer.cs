using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Linq;
using Terraria.GameInput;
using Terraria.Enums;

namespace ArknightsMod.Common.Players
{
	public class ShakeEffectPlayer : ModPlayer
	{
		public int screenShakeTime = 0;//屏幕抖动时间
		public Vector2 screenShakeModifier = Vector2.Zero;//常规为0
		public Vector2 screenShakeVelocity = Vector2.One;//常规为1
		public bool screenShakeOnlyOnY = false;//纵向振动，常规为否


        public override void ModifyScreenPosition()//改变视角位置
		{
			Main.screenPosition += screenShakeModifier;
		}

		public override void PostUpdate ()//动态更新
		{
			float maxScreenShakeDistance = 20;
			float screenShakeSpeed = 10;

			if (screenShakeTime > 0)//生效
			{
				if(screenShakeOnlyOnY == true)//只在y轴进行
				{
					maxScreenShakeDistance = 5;
					screenShakeSpeed = 4;
					screenShakeModifier.Y += screenShakeVelocity.Y;//震动位移被震动速度所改变，这里只改变Y
					screenShakeVelocity.Normalize();//将震动速度归位
					screenShakeVelocity *= screenShakeSpeed;//震动速度乘以倍率
					if (screenShakeModifier.Length() >= maxScreenShakeDistance)//在震动位移超出上限时
					{
						screenShakeModifier.Normalize();//震动位移归位
						screenShakeModifier *= maxScreenShakeDistance;//震动位移乘以最大震动位移
						screenShakeVelocity = -screenShakeSpeed * screenShakeModifier.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5);//震动速度变为反向并不断减少
					}
				}
			}
			else//归位
			{
				screenShakeModifier = screenShakeModifier.SafeNormalize(Vector2.Zero) * Math.Max(screenShakeModifier.Length() - screenShakeSpeed, 0);//震动位移逐渐归位
			}
		}

        public override void ResetEffects()//重置效果
		{
			//屏幕抖动时间减少
			if (screenShakeTime > 0)
			{
				screenShakeTime--;
			}
		}
	}
}