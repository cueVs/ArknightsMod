using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
	// 干员时装与套装工具提示的统一排版：限制单行最大相对长度并手动换行（游戏内长文本不会自动折行）。
	public static class OperatorOutfitTooltipLayout
	{
		// 相对默认可读宽度的单行字符上限（中文全角按 1 计）。
		public const float MaxRelativeLineWidth = 0.92f;

		// 基准单行字数，实际限制 = floor(基准 * MaxRelativeLineWidth)。
		public const int BaseCharsPerLine = 40;

		public static int MaxCharsPerLine =>
			System.Math.Max(16, (int)(BaseCharsPerLine * MaxRelativeLineWidth));

		public static readonly Color EffectTextColor = new(190, 235, 255);

		public static IEnumerable<string> WrapLines(string text) {
			if (string.IsNullOrEmpty(text))
				yield break;

			foreach (string paragraph in text.Split('\n')) {
				string remaining = paragraph.Trim();
				while (GetVisibleLength(remaining) > MaxCharsPerLine) {
					int breakIndex = FindBreakIndex(remaining, MaxCharsPerLine);
					yield return remaining[..breakIndex];
					remaining = remaining[breakIndex..].TrimStart();
				}

				if (remaining.Length > 0)
					yield return remaining;
			}
		}

		private static int FindBreakIndex(string text, int maxVisibleLength) {
			int limit = System.Math.Min(text.Length, text.Length);
			for (int i = text.Length; i > 0; i--) {
				if (GetVisibleLength(text[..i]) <= maxVisibleLength) {
					limit = i;
					break;
				}
			}

			for (int i = limit - 1; i >= limit / 2; i--) {
				char c = text[i];
				if (c is '，' or '。' or '；' or '、' or '：' or '）' or ')' or ' ' or ',' or '.' or ';')
					return i + 1;
			}

			return limit;
		}

		private static int GetVisibleLength(string text) {
			var builder = new StringBuilder();
			bool inTag = false;
			foreach (char c in text) {
				if (c == '[')
					inTag = true;
				else if (c == ']')
					inTag = false;
				else if (!inTag)
					builder.Append(c);
			}

			return builder.Length;
		}

		public static void AddWrappedEffectLines(Mod mod, List<TooltipLine> tooltips, string localizationKey, string tooltipLineId) {
			string text = Language.GetTextValue(localizationKey);
			int lineIndex = 0;
			foreach (string line in WrapLines(text)) {
				tooltips.Add(new TooltipLine(mod, $"{tooltipLineId}_{lineIndex}", line) {
					OverrideColor = EffectTextColor
				});
				lineIndex++;
			}
		}

		// 时装等物品的通用长文本折行。
		public static void ApplyWrappedVanityLine(Mod mod, List<TooltipLine> tooltips, string localizationKey) {
			AddWrappedEffectLines(mod, tooltips, localizationKey, "VanityHint");
		}

		// 替换以 lineIdPrefix 开头的折行效果行（用于套装激活后的动态数值）。
		public static void ReplaceWrappedLines(Mod mod, List<TooltipLine> tooltips, string lineIdPrefix, string text) {
			tooltips.RemoveAll(line => line.Name.StartsWith(lineIdPrefix));
			int lineIndex = 0;
			foreach (string line in WrapLines(text)) {
				tooltips.Add(new TooltipLine(mod, $"{lineIdPrefix}_{lineIndex}", line) {
					OverrideColor = EffectTextColor
				});
				lineIndex++;
			}
		}

		// 在指定位置插入折行后的提示行。
		public static void InsertWrappedLines(
			Mod mod,
			List<TooltipLine> tooltips,
			int insertIndex,
			string text,
			string lineIdPrefix,
			Color? color = null) {
			int offset = 0;
			Color lineColor = color ?? Color.White;
			foreach (string line in WrapLines(text)) {
				tooltips.Insert(insertIndex + offset, new TooltipLine(mod, $"{lineIdPrefix}_{offset}", line) {
					OverrideColor = lineColor
				});
				offset++;
			}
		}
	}
}
