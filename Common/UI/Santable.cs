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
		private int CurrentSan = Main.LocalPlayer.GetModPlayer<San>().CurrentSan;
		public static bool Visible = true;

		public override void OnInitialize() {
			UIPanel panel = new UIPanel();
			Append(panel);
		}


		public override void Draw(SpriteBatch spriteBatch) {
			if (1000 >= CurrentSan && CurrentSan >= 916) {
				spriteBatch.Draw(ModContent.Request<Texture2D>("UI/Santable").Value, new Vector2(400, 300), new Rectangle(0, 43 * 12, 43, 43), Color.White, 0, new Vector2(20, 20), 2f, 0, 0);
			}
		}
		
	}
}
