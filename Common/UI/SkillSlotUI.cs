using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ArknightsMod.Common.UI
{
	public class SkillSlotUI : UIImage
	{
		private SkillData skillData;
		private readonly static Texture2D hoverBG = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillHoverBackGround", AssetRequestMode.ImmediateLoad).Value;
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

			initSP = new(string.Empty, 0.75f);
			initSP.Left.Set(18, 0);
			initSP.Top.Set(56, 0);
			Append(initSP);

			maxSP = new(string.Empty, 0.75f);
			maxSP.Left.Set(48, 0);
			maxSP.Top.Set(56, 0);
			Append(maxSP);
		}
		protected override void DrawSelf(SpriteBatch sb) {
			base.DrawSelf(sb);

			if (IsMouseHovering) {
				Main.LocalPlayer.mouseInterface = true;
				if (skillData == null)
					return;
				var font = FontAssets.MouseText.Value;
				const float maxWidth = 420f;
				// 中文没有空格，原版的按空格换行不会生效，导致描述整行超长溢出；
				// 这里按字符宽度手动折行（CJK 逐字断行）。标题不折行，描述折行。
				string tips = skillData.Label.Value + "\n" + WrapCjk(font, skillData.Desc.Value, maxWidth);
				if (Main.LocalPlayer.HeldItem.ModItem is UpgradeWeaponBase ark) {
					string activateKey = ark.GetSkillActivateKeyHint();
					if (!string.IsNullOrEmpty(activateKey))
						tips += "\n" + WrapCjk(font, activateKey, maxWidth);
				}
				string[] lines = tips.Replace("\r", string.Empty).Split('\n');
				int width = 0;
				int height = 0;
				for (int i = 0; i < lines.Length; i++) {
					string line = lines[i];
					Point lineSize = ChatManager.GetStringSize(font, string.IsNullOrEmpty(line) ? " " : line, Vector2.One, maxWidth).ToPoint();
					if (lineSize.X > width)
						width = lineSize.X;
					height += lineSize.Y;
				}
				Rectangle area = new(20, 220, width + 30, height + 20);
				sb.Draw(hoverBG, area, Color.White);
				Vector2 drawPos = new(30, 230);
				for (int i = 0; i < lines.Length; i++) {
					string line = lines[i];
					var snippets = ChatManager.ParseMessage(line, Color.White).ToArray();
					ChatManager.DrawColorCodedString(sb, font, snippets,
						drawPos, Color.White, 0f, Vector2.Zero, Vector2.One, out _, maxWidth);
					Point lineSize = ChatManager.GetStringSize(font, string.IsNullOrEmpty(line) ? " " : line, Vector2.One, maxWidth).ToPoint();
					drawPos.Y += lineSize.Y;
				}
			}
		}

		// 按字符宽度逐字折行（适配中文等无空格文本）。保留原文里已有的换行符。
		// 颜色代码 tag（[c/hex:文本]）作为不可分割单元处理：折行永远不会打断 tag，
		// 否则被拆散的 [c/ 与 ] 无法被 ChatManager 的正则匹配（. 不跨行），颜色会原样显示。
		// tag 的宽度按冒号后实际显示的内容计算（[c/、hex、:、] 本身不渲染）。
		private static string WrapCjk(ReLogic.Graphics.DynamicSpriteFont font, string text, float maxWidth) {
			if (string.IsNullOrEmpty(text))
				return text;
			var result = new System.Text.StringBuilder(text.Length + 16);
			float lineWidth = 0f;
			int i = 0;
			while (i < text.Length) {
				char c = text[i];
				if (c == '\n') {
					result.Append(c);
					lineWidth = 0f;
					i++;
					continue;
				}
				// 识别颜色代码 tag：[c/hex:文本]
				if (c == '[' && i + 2 < text.Length && (text[i + 1] == 'c' || text[i + 1] == 'C') && text[i + 2] == '/') {
					int close = text.IndexOf(']', i + 2);
					if (close >= 0) {
						int colon = text.IndexOf(':', i + 3);
						string tag = text.Substring(i, close - i + 1);
						// 只测量冒号后实际显示的内容；找不到冒号时退化为测量整个 tag
						string displayText = (colon >= 0 && colon < close)
							? text.Substring(colon + 1, close - colon - 1)
							: tag;
						float tagWidth = font.MeasureString(displayText).X;
						if (lineWidth > 0f && lineWidth + tagWidth > maxWidth) {
							result.Append('\n');
							lineWidth = 0f;
						}
						result.Append(tag);
						lineWidth += tagWidth;
						i = close + 1;
						continue;
					}
				}
				float cw = font.MeasureString(c.ToString()).X;
				if (lineWidth > 0f && lineWidth + cw > maxWidth) {
					result.Append('\n');
					lineWidth = 0f;
				}
				result.Append(c);
				lineWidth += cw;
				i++;
			}
			return result.ToString();
		}

		protected override void DrawChildren(SpriteBatch sb) {
			if (skillData == null)
				return;
			base.DrawChildren(sb);
			if (Main.LocalPlayer.GetModPlayer<WeaponPlayer>().CurrentSkill == skillData) {
				sb.Draw(selector, GetDimensions().ToRectangle().TopRight() - Vector2.UnitX * 20, Color.White);
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
