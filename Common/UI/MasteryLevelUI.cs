using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace ArknightsMod.Common.UI
{
	public class MasteryLevelUI : UIImage
	{
		private readonly static Texture2D masteryLevel = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/MasteryLevel", AssetRequestMode.ImmediateLoad).Value;
		private Rectangle source = new(-1, 0, 16, 16);
		public MasteryLevelUI() : base(masteryLevel) {
			Left.Set(-4, 0);
			Top.Set(-6, 0);
			Width.Set(16, 0);
			Height.Set(16, 0);
		}
		protected override void DrawSelf(SpriteBatch sb) {
			if (source.X < 0)
				return;
			Vector2 pos = GetDimensions().ToRectangle().TopLeft();
			sb.Draw(masteryLevel, pos, source, Color.White);
		}
		public void SetLevel(int level) {
			int x;
			level -= 7;
			if (level < 1)
				x = -1;
			else {
				x = (level - 1) * 16;
			}
			source.X = x;
		}
	}
}
