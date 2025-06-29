using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;

namespace ArknightsMod.Common.UI.BattleRecord.UIElements
{
	internal class UIBlock : UIImage
	{
		public UIBlock() : base(TextureAssets.MagicPixel) {
			Color = new Microsoft.Xna.Framework.Color(71, 71, 71);
			ScaleToFit = true;
		}
	}
}