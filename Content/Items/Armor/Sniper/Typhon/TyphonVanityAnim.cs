using Terraria;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	internal static class TyphonVanityAnim
	{
		public const int AtlasCellWidth = 40;
		public const int AtlasCellHeight = 56;

		public static readonly int[] HornOverlayAtlasRows = { 7, 8, 9, 14, 15, 16 };

		public static bool BodyFrameMatchesHornsLongStrip(Player p)
		{
			if (p.bodyFrame.Width != AtlasCellWidth || p.bodyFrame.Height != AtlasCellHeight)
				return false;
			int row = p.bodyFrame.Y / AtlasCellHeight;
			foreach (int r in HornOverlayAtlasRows) {
				if (row == r)
					return true;
			}

			return false;
		}
	}
}
