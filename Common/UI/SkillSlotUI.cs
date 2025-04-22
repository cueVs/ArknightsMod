using ArknightsMod.Common.Items;
using ArknightsMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace ArknightsMod.Common.UI
{
	public class SkillSlotUI : UIImage
	{
		private SkillData skillData;

		private readonly static Texture2D noSkill = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/NoSkill", AssetRequestMode.ImmediateLoad).Value;
		private readonly static Texture2D baseOfSP = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BaseOfSP", AssetRequestMode.ImmediateLoad).Value;
		private readonly static Texture2D selector = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillSelector", AssetRequestMode.ImmediateLoad).Value;
		private readonly MasteryLevelUI ml;
		private readonly UIText initSP, maxSP;
		private readonly UIImage icon;
		public SkillSlotUI() : base(noSkill) {
			icon = new(noSkill);
			Append(icon);

			ml = new();
			Append(ml);

			UIImage sp = new(baseOfSP);
			sp.Left.Set(6, 0);
			sp.Top.Set(52, 0);
			Append(sp);

			initSP = new(string.Empty);
			initSP.Left.Set(18, 0);
			initSP.Top.Set(52, 0);
			Append(initSP);

			maxSP = new(string.Empty);
			maxSP.Left.Set(48, 0);
			maxSP.Top.Set(52, 0);
			Append(maxSP);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		protected override void DrawChildren(SpriteBatch sb) {
			if (skillData == null)
				return;
			base.DrawChildren(sb);
			if (Main.LocalPlayer.GetModPlayer<WeaponPlayer>().CurrentSkill == skillData) {
				sb.Draw(selector, GetDimensions().ToRectangle().TopRight() - Vector2.UnitX * 14, Color.White);
			}
		}
		public void SetSkill(SkillData value) {

			skillData = value;
			if (value == null)
				return;
			int level = value.ForceReplaceLevel ?? value.Level;
			ml.SetLevel(level);
			SkillLevelData data = value[level];
			initSP.SetText(data.InitSP.ToString());
			maxSP.SetText(data.MaxSP.ToString());
			icon.SetImage(skillData.Icon.Value);
		}
		public SkillData GetSkill() => skillData;
	}
}
