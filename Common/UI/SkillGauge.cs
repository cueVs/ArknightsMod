using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI
{
	internal class SkillGauge : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private UIText text;

		private UIElement area;
		private UIImage barFrame;
		private Color gradientA;
		private Color gradientB;
		private Color skillColor;
		private const int ChargePulseActiveTicks = 36;
		private const int ChargePulsePauseTicks = 12;
		private const int ChargePulseCycleTicks = ChargePulseActiveTicks + ChargePulsePauseTicks;
		private const float ChargePulseExpandMax = 1.75f;
		private static readonly Color ChargePulseColor = new(255, 220, 0);
		private readonly Texture2D[] stockIcon = [
			ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock1", AssetRequestMode.ImmediateLoad).Value,
			ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock2", AssetRequestMode.ImmediateLoad).Value,
			ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock3", AssetRequestMode.ImmediateLoad).Value,
		];
		private readonly Texture2D skillCanUse =
			ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Skill", AssetRequestMode.ImmediateLoad).Value;

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element.
			// UIElement is invisible and has no padding.
			area = new UIElement();
			//area.Left.Set(-area.Width.Pixels - 790, 1f); // Place the resource bar to the left of the hearts.
			area.Top.Set(70, 0f); // Placing it just a bit below the top of the screen.
			area.Width.Set(139, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
			area.Height.Set(20, 0f);
			area.VAlign = 0.5f;
			area.HAlign = 0.5f;

			barFrame = new UIImage(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillGaugeFrame")); // Frame of our resource bar
			barFrame.Left.Set(10, 0f);
			barFrame.Top.Set(0, 0f);
			barFrame.Width.Set(118, 0f);
			barFrame.Height.Set(12, 0f);

			text = new UIText("0/0", 0.8f); // text to show stat
			text.Width.Set(118, 0f);
			text.Height.Set(12, 0f);
			text.Top.Set(100, 0f);
			text.Left.Set(0, 0f);

			gradientA = new Color(181, 191, 100); // A light green
			gradientB = new Color(127, 114, 96); // A gray

			skillColor = new Color(255, 197, 0);

			//area.Append(text);
			area.Append(barFrame);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// This prevents drawing unless we are using an ExampleCustomResourceWeapon
			if (Main.LocalPlayer.HeldItem.ModItem is not UpgradeWeaponBase)
				return;
			base.Draw(spriteBatch);
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch sb) {
			base.DrawSelf(sb);

			var mp = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			SkillData skill = mp.CurrentSkill;

			//null reference check
			if (skill == null) {
				//Main.NewText($"[{GetType()}] 错误: 当前技能数据mp.CurrentSkill为null", Color.Red);
				return;
			}

			SkillLevelData data = skill.CurrentLevelData;

			// 必须和 WeaponPlayer.UpdateActiveSkill 里判断"技能到期"的阈值用同一个倍率，
			// 否则开了"技能持续时间技力消耗二倍速"后，进度条按 1 倍速的总长度画，
			// 而技能在一半处就结束了，表现为条子掉到一半突然清空。
			float activeTime = data.ActiveTime * 60 * WeaponPlayer.ActiveDurationMultiplier;
			int maxStock = data.MaxStack;
			int stock = mp.StockCount;

			//确保数值有效
			if (activeTime <= 0)
				activeTime = 1f;
			if (maxStock <= 0)
				maxStock = 1;

			//Calculate quotient
			float quotient1 = (float)mp.SkillCharge / mp.SkillChargeMax;
			quotient1 = Utils.Clamp(quotient1, 0f, 1f);
			float quotient2 = mp.SkillTimer / activeTime;
			quotient2 = Utils.Clamp(quotient2, 0f, 1f);

			//检查UI元素是否初始化
			if (barFrame == null) {
				Main.NewText("错误: 技能UI的barFrame元素未初始化", Color.Red);
				return;
			}

			Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 2;
			hitbox.Width -= 2;
			hitbox.Y += 2;
			hitbox.Height -= 2;

			var aboveHead = new Rectangle(Main.screenWidth / 2 - 12, Main.screenHeight / 2 - 65, 22, 22);

			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps1 = (int)((right - left) * quotient1);
			int steps2 = (int)((right - left) * quotient2);

			//绘制技能条背景和填充
			sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), gradientB);
			for (int i = 0; i < steps1; i += 1) {
				sb.Draw(pixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), gradientA);
			}
			if (mp.StockCount == maxStock) {
				sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), gradientA);
			}

			if (mp.SkillActive) {
				sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), skillColor);
				// 永久技能展开后保持整条金色，不画并不存在的倒计时。
				if (!skill.IsPermanent) {
					for (int i = 0; i < steps2; i += 1) {
						sb.Draw(pixel, new Rectangle(right - i, hitbox.Y, 1, hitbox.Height), gradientB);
					}
				}
			}

			//检查贴图资源是否存在
			if (maxStock > 1 && stock > 0) {
				if (stockIcon != null && stockIcon.Length >= stock && stockIcon[stock - 1] != null) {
					if (stock == maxStock && !skill.SuppressReadyPulse)
						DrawChargeReadyPulse(sb, aboveHead);
					sb.Draw(stockIcon[stock - 1], aboveHead, Color.White);
				}
			}
			else if (maxStock == 1 && !skill.AutoTrigger) {
				if (stock == 1 && skillCanUse != null && !mp.SkillActive) {
					DrawChargeReadyPulse(sb, aboveHead);
					sb.Draw(skillCanUse, aboveHead, Color.White);
				}
			}
		}

		private static void DrawChargeReadyPulse(SpriteBatch sb, Rectangle iconRect) {
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			int tickInCycle = (int)(Main.GameUpdateCount % ChargePulseCycleTicks);
			if (tickInCycle >= ChargePulseActiveTicks)
				return;

			float progress = tickInCycle / (float)ChargePulseActiveTicks;
			float alpha = 1f - progress;
			if (alpha <= 0f)
				return;

			float expand = MathHelper.Lerp(0.55f, ChargePulseExpandMax, progress);
			float side = iconRect.Width * expand;
			Vector2 center = iconRect.Center.ToVector2();
			Color color = ChargePulseColor * alpha;

			sb.Draw(
				pixel,
				center,
				new Rectangle(0, 0, 1, 1),
				color,
				MathHelper.PiOver4,
				new Vector2(0.5f, 0.5f),
				new Vector2(side, side),
				SpriteEffects.None,
				0f);
		}

		//public override void Update(GameTime gameTime) {
		//	if (Main.LocalPlayer.HeldItem.ModItem is not KroosCrossbow)
		//		return;

		//	var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
		//	// Setting the text per tick to update and show our resource values.
		//	text.SetText($"SP: {modPlayer.SP} / {modPlayer.MaxSP}");
		//	base.Update(gameTime);
		//}
	}

	internal class SkillGaugeSystem : ModSystem
	{
		private UserInterface SkillGaugeUserInterface;

		internal SkillGauge SkillGauge;

		public override void Load() {
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
				SkillGauge = new();
				SkillGaugeUserInterface = new();
				SkillGaugeUserInterface.SetState(SkillGauge);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			SkillGaugeUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"ArknightsMod: Skill Gauge",
					delegate {
						SkillGaugeUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
