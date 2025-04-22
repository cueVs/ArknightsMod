using ArknightsMod.Common.Items;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
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
			if (Main.LocalPlayer.HeldItem.ModItem is not ArknightsWeapon)
				return;
			base.Draw(spriteBatch);
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch sb) {
			base.DrawSelf(sb);

			var mp = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			SkillData skill = mp.CurrentSkill;
			SkillLevelData data = skill.CurrentLevelData;
			float activeTime = data.ActiveTime * 60;
			int maxStock = data.MaxStock;
			int stock = mp.StockCount;
			// Calculate quotient
			float quotient1 = (float)mp.SkillCharge / mp.SkillChargeMax; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient1 = Utils.Clamp(quotient1, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.
			float quotient2 = mp.SkillTimer / activeTime; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient2 = Utils.Clamp(quotient2, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.



			// Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
			Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 2;
			hitbox.Width -= 2;
			hitbox.Y += 2;
			hitbox.Height -= 2;

			var aboveHead = new Rectangle(Main.screenWidth / 2 - 12, Main.screenHeight / 2 - 65, 22, 22);

			// Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps1 = (int)((right - left) * quotient1);
			int steps2 = (int)((right - left) * quotient2);

			sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), gradientB);
			for (int i = 0; i < steps1; i += 1) {
				sb.Draw(pixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), gradientA);
			}
			if (mp.StockCount == maxStock) {
				sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), gradientA);
			}

			if (mp.SkillActive) {
				sb.Draw(pixel, new Rectangle(left, hitbox.Y, 116, hitbox.Height), skillColor);
				for (int i = 0; i < steps2; i += 1) {
					sb.Draw(pixel, new Rectangle(right - i, hitbox.Y, 1, hitbox.Height), gradientB);
				}
			}

			if (maxStock > 1 && stock > 0) {
				sb.Draw(stockIcon[stock - 1], aboveHead, Color.White);
			}
			else if (maxStock == 1 && !skill.AutoTrigger) {
				if (stock == 1) {
					sb.Draw(skillCanUse, aboveHead, Color.White);
				}
			}
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

	class SkillGaugeSystem : ModSystem
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
