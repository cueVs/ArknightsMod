using ArknightsMod.Common.UI.BattleRecord.Calculators;
using ArknightsMod.Content.Items;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Common.UI.BattleRecord.UIElements
{
	internal class UIUpgradePreview : UIBlock
	{
		public UpgradeItemBase UpgradeItem;
		public BattleRecordCalculator BattleRecordCalculator;
		public ExperienceCalculator ExperienceCalculator;

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			UpgradeItem?.DrawUpgradePreview(spriteBatch, GetDimensions().ToRectangle(), BattleRecordCalculator, ExperienceCalculator);
		}
	}
}