using ArknightsMod.Content.Events;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;

public class EventProgressBar : UIState
{
	private UIText progressText;
	private UIElement progressBar;
	private float width = 400f;
	private float height = 30f;

	public override void OnInitialize() {
		progressBar = new UIElement {
			Width = new StyleDimension(width, 0f),
			Height = new StyleDimension(height, 0f),
			Top = new StyleDimension(50f, 0f),
			Left = new StyleDimension(-width / 2, 0.5f)
		};

		progressText = new UIText("", 0.8f) {
			Top = new StyleDimension(height + 5f, 0f),
			HAlign = 0.5f
		};

		Append(progressBar);
		Append(progressText);
	}
	public void ResetPosition() {
		// 重置为初始位置（屏幕水平居中，顶部下移50像素）
		progressBar.Left = new StyleDimension(-width / 2, 0.5f);
		progressBar.Top = new StyleDimension(50f, 0f);

		// 强制重新计算布局
		progressBar.Recalculate();
		Recalculate(); // 如果progressBar有父容器也需要刷新

		// 调试输出验证
		var rect = progressBar.GetDimensions().ToRectangle();
	}
	protected override void DrawSelf(SpriteBatch spriteBatch) {
		if (!UnionInvade.EventActive)
			return;

		float progress = (float)UnionInvade.MonstersKilled / UnionInvade.MonstersRequired;
		progress = MathHelper.Clamp(progress, 0f, 1f);

		// 绘制背景
		var bgColor = new Color(0, 0, 0, 180);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			progressBar.GetDimensions().ToRectangle(),
			bgColor
		);

		// 绘制进度条
		var fillColor = Color.Lerp(Color.Red, Color.LimeGreen, progress);
		var fillRect = new Rectangle(
			(int)progressBar.GetDimensions().X,
			(int)progressBar.GetDimensions().Y,
			(int)(width * progress),
			(int)height
		);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			fillRect,
			fillColor
		);

		// 绘制边框
		var borderColor = new Color(255, 255, 255, 150);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)progressBar.GetDimensions().X, (int)progressBar.GetDimensions().Y, (int)width, 2),
			borderColor
		);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)progressBar.GetDimensions().X, (int)(progressBar.GetDimensions().Y + height - 2), (int)width, 2),
			borderColor
		);

		// 更新文本
		progressText.SetText($"作战进度: {UnionInvade.MonstersKilled}/{UnionInvade.MonstersRequired}");
	}
	public class EventUISystem : ModSystem
	{
		internal UserInterface eventInterface;
		internal EventProgressBar progressBar;

		public override void Load() {
			progressBar = new EventProgressBar();
			eventInterface = new UserInterface();
			eventInterface.SetState(progressBar);
		}

		public override void UpdateUI(GameTime gameTime) {
			if (UnionInvade.EventActive) {
				eventInterface?.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int index = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"YourMod: Event Progress",
					delegate {
						if (UnionInvade.EventActive) {
							eventInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
