using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
	public class ModifyScreenPosPlayer : ModPlayer
	{
		public bool modifyScreenPos;
		public Vector2 ScreenPosition;
		public override void ResetEffects()
		{
			modifyScreenPos = false;
		}

		public override void ModifyScreenPosition() {
			if (modifyScreenPos)
				Main.screenPosition = ScreenPosition - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
		}
	}
}

