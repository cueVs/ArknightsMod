﻿using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;

namespace ArknightsMod.VisualEffects
{
	public class ArknightsModMenu : ModMenu
	{
		private const string menuAssetPath = "ArknightsMod/VisualEffects/Menu";

		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>($"{menuAssetPath}/TerraArk");

        public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>($"{menuAssetPath}/mrfz");

		public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>($"{menuAssetPath}/Sami2");

		public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Sounds/Music/arkopen");

		public override string DisplayName => "泰拉方舟 Terra Arknights";

		public override void OnSelected()
		{
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Content/Sounds/AmiyaArknights"));
		}
	}
}
