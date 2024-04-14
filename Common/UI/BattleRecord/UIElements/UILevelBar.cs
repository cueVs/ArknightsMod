using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace ArknightsMod.Common.UI.BattleRecord.UIElements
{
	internal class UILevelBar : UIElement
	{
		public float ExperiencePercent = 0;
		public float PreviewExperiencePercent = 0;

		public override void Draw(SpriteBatch spriteBatch) {
			int borderWidth = 2;
			Color borderColor = Color.Black;
			Texture2D texture = TextureAssets.MagicPixel.Value;
			var rectangle = GetDimensions().ToRectangle();

			Color expColor = Color.Lerp(new Color(255, 212, 40), Color.Gray, 0.2f);
			Color expPreviewColor = new Color(250, 207, 3);
			var expRectangle = new Rectangle(rectangle.X + borderWidth, rectangle.Y + borderWidth,
				 rectangle.Width - borderWidth * 2, rectangle.Height - borderWidth * 2);
			var expPreviewRectangle = expRectangle;

			expPreviewRectangle.Width = (int)(PreviewExperiencePercent * expRectangle.Width);
			expRectangle.Width = (int)(ExperiencePercent * expRectangle.Width);
			expPreviewRectangle.X += expRectangle.Width;

			spriteBatch.Draw(texture, expRectangle, expColor);
			spriteBatch.Draw(texture, expPreviewRectangle, expPreviewColor);

			spriteBatch.Draw(texture,
				new Rectangle(rectangle.X, rectangle.Y, borderWidth, rectangle.Height - borderWidth),
				borderColor);
			spriteBatch.Draw(texture,
				new Rectangle(rectangle.X + borderWidth, rectangle.Y, rectangle.Width - borderWidth, borderWidth),
				borderColor);
			spriteBatch.Draw(texture,
				new Rectangle(rectangle.Right - borderWidth, rectangle.Y + borderWidth, borderWidth, rectangle.Height - borderWidth),
				borderColor);
			spriteBatch.Draw(texture,
				new Rectangle(rectangle.X, rectangle.Bottom - borderWidth, rectangle.Width - borderWidth, borderWidth),
				borderColor);
		}
	}
}