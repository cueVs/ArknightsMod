using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameInput;
using log4net.Core;
using ArknightsMod;



namespace ArknightsMod.Common.UI
{
	public class Santable : UIState
	{
		
		public static bool Visible = true;

		public override void OnInitialize() {
			UIPanel panel = new UIPanel();
			Append(panel);
		}
		private int CurrenSan;
		private int SanCD;

		public override void Draw(SpriteBatch spriteBatch) {
			CurrenSan = Main.LocalPlayer.GetModPlayer<San>().CurrentSan;
			SanCD = Main.LocalPlayer.GetModPlayer<San>().MadnessCD;

			if (1000 >= CurrenSan && CurrenSan >= 916) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth*0.775f), (Main.screenHeight*0.1f)), new Rectangle(0, 43 * 12, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (916 > CurrenSan && CurrenSan >= 833) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 11, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (833 > CurrenSan && CurrenSan >= 750) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 10, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (750 > CurrenSan && CurrenSan >= 666) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 9, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (666 > CurrenSan && CurrenSan >= 583) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 8, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (583 > CurrenSan && CurrenSan >= 500) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 7, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (500 > CurrenSan && CurrenSan >= 416) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 6, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (416 > CurrenSan && CurrenSan >= 333) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 5, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (333 > CurrenSan && CurrenSan >= 250) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 4, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (250 > CurrenSan && CurrenSan >= 167) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 3, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (167 > CurrenSan && CurrenSan >= 83) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 2, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (83 > CurrenSan && CurrenSan > 0) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 43 * 1, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (SanCD <= 601 && CurrenSan == 1000) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2((Main.screenWidth * 0.775f), (Main.screenHeight * 0.1f)), new Rectangle(0, 0, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (1000 > CurrenSan && CurrenSan >= 950) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 20, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (950 >= CurrenSan && CurrenSan >= 900) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 19, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (900 >= CurrenSan && CurrenSan >= 850) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 18, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (850 >= CurrenSan && CurrenSan >= 800) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 17, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (800 >= CurrenSan && CurrenSan >= 750) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 16, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (750 >= CurrenSan && CurrenSan >= 700) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 15, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (700 >= CurrenSan && CurrenSan >= 650) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 14, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (650 >= CurrenSan && CurrenSan >= 600) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 13, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (600 >= CurrenSan && CurrenSan >= 550) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 12, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (550 >= CurrenSan && CurrenSan >= 500) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 11, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (500 >= CurrenSan && CurrenSan >= 450) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 10, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (450 >= CurrenSan && CurrenSan >= 400) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 9, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (400 >= CurrenSan && CurrenSan >= 350) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 8, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (350 >= CurrenSan && CurrenSan >= 300) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 7, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (300 >= CurrenSan && CurrenSan >= 250) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 6, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (250 >= CurrenSan && CurrenSan >= 200) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 5, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (200 >= CurrenSan && CurrenSan >= 150) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 4, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (150 >= CurrenSan && CurrenSan >= 100) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 3, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (100 >= CurrenSan && CurrenSan >= 50) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 2, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (50 >= CurrenSan && CurrenSan >= 0) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 1, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}
			if (SanCD <= 601 && CurrenSan == 1000) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SmallSantable").Value, new Vector2((Main.screenWidth * 0.5f), (Main.screenHeight * 0.5f + 40)), new Rectangle(0, 19 * 0, 20, 20), Color.White, 0, new Vector2(10, 10), 1f, 0, 0);
			}

		}
		
	}
}
