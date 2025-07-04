using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Levels
{
	public class Level11 : ModNPC
	{
		public override string Texture => "ArknightsMod/Content/NPCs/Levels/Rhodes";
		public override void SetDefaults() {
			NPC.dontTakeDamage = true;
			NPC.friendly = false;
			NPC.width = 1;
			NPC.height = 1;
			NPC.lifeMax = 1;
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

		}

		private float rotation;
		private float circleRadius = 200f;       // 圆环半径
		private int points = 32;                // 圆周点数
		private Color circleColor = Color.Blue;  // 线颜色
		private float lineWidth = 2f;            // 线宽
		private float animationSpeed = 0.02f;     // 动画速度
		private float rotationAngle;

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// 1. 获取NPC中心在屏幕上的位置
			Vector2 centerScreen = NPC.Center - screenPos;
			Texture2D circleTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Levels/Rhodes").Value;
			//spriteBatch.Draw(circleTexture,NPC.Center,)
			
		}
	}

}


