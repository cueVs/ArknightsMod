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
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 12, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (916 > CurrenSan && CurrenSan >= 833) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 11, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (833 > CurrenSan && CurrenSan >= 750) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 10, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (750 > CurrenSan && CurrenSan >= 666) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 9, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (666 > CurrenSan && CurrenSan >= 583) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 8, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (583 > CurrenSan && CurrenSan >= 500) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 7, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (500 > CurrenSan && CurrenSan >= 416) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 6, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (416 > CurrenSan && CurrenSan >= 333) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 5, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (333 > CurrenSan && CurrenSan >= 250) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 4, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (250 > CurrenSan && CurrenSan >= 167) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 3, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (167 > CurrenSan && CurrenSan >= 83) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 2, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (83 > CurrenSan && CurrenSan > 0) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 43 * 1, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}
			if (SanCD <= 601) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Santable").Value, new Vector2(1400, 70), new Rectangle(0, 0, 43, 43), Color.White, 0, new Vector2(20, 20), 1.5f, 0, 0);
			}


		}
		
	}
}
