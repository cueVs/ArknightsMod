using ArknightsMod.Common.UI.BattleRecord;
using ArknightsMod.Common.UI.BattleRecord.Calculators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons
{
	public abstract class UpgradeWeaponBase : UpgradeItemBase
	{
		public override void DrawUpgradePreview(SpriteBatch spriteBatch, Rectangle rectangle, BattleRecordCalculator battleRecordCalculator, ExperienceCalculator experienceCalculator) {
			var pos = rectangle.Location.ToVector2();
			pos.Y += 20f;
			var previewLevel = Level + experienceCalculator.UpgradeLevelPreview;
			int nowDamage = GetDamage(Level);
			int previewDamage = GetDamage(previewLevel);

			var font = FontAssets.MouseText.Value;
			string text = Language.GetTextValue("Mods.ArknightsMod.UpgradeWeapon.LevelPreview.Damage");
			Vector2 textSize = font.MeasureString(text);
			pos.X = rectangle.X + (rectangle.Width - textSize.X) / 2f;
			spriteBatch.DrawString(font, text, pos, Color.White);
			pos.Y += textSize.Y;

			text = Level == previewLevel ? $"{nowDamage}" : $"{nowDamage}->{previewDamage}";
			textSize = font.MeasureString(text);
			pos.X = rectangle.X + (rectangle.Width - textSize.X) / 2f;
			spriteBatch.DrawString(font, text, pos, Color.White);
			pos.Y += textSize.Y;
		}

		protected abstract int GetDamage(int level);
	}
}