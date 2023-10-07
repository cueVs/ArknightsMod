using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
using Terraria.GameContent;
using System.Collections.Generic;

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

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.
			area = new UIElement();
			//area.Left.Set(-area.Width.Pixels - 790, 1f); // Place the resource bar to the left of the hearts.
			area.Top.Set(55, 0f); // Placing it just a bit below the top of the screen.
			area.Width.Set(139, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
			area.Height.Set(60, 0f);
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
			if (Main.LocalPlayer.HeldItem.ModItem is BagpipeSpear) {
				base.Draw(spriteBatch);
			}
			if (Main.LocalPlayer.HeldItem.ModItem is KroosCrossbow) {
				base.Draw(spriteBatch);
			}
			if (Main.LocalPlayer.HeldItem.ModItem is ChenSword) {
				base.Draw(spriteBatch);
			}
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			// Calculate quotient
			float quotient1 = (float)modPlayer.SkillCharge / modPlayer.SkillChargeMax; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient1 = Utils.Clamp(quotient1, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.
			float quotient2 = (float)modPlayer.SkillTimer / (modPlayer.SkillActiveTime * 60); // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient2 = Utils.Clamp(quotient2, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.



			// Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
			Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 2;
			hitbox.Width -= 4;
			hitbox.Y += 2;
			hitbox.Height -= 4;

			var aboveHead = new Rectangle((int)Main.screenWidth / 2 - 12, (int)Main.screenHeight / 2 - 65, 22, 22);
			Texture2D skillStock1 = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock1").Value;
			Texture2D skillStock2 = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock2").Value;
			Texture2D skillStock3 = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillStock3").Value;
			Texture2D skill = ModContent.Request<Texture2D>("ArknightsMod/Common/UI/Skill").Value;

			// Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps1 = (int)((right - left) * quotient1);
			int steps2 = (int)((right - left) * quotient2);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left, hitbox.Y, 114, hitbox.Height), gradientB);
			for (int i = 0; i < steps1; i += 1) {
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), gradientA);
			}
			if (modPlayer.StockCount == modPlayer.StockMax) {
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left, hitbox.Y, 114, hitbox.Height), gradientA);
			}

			if (modPlayer.SkillActive) {
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left, hitbox.Y, 114, hitbox.Height), skillColor);
				for (int i = 0; i < steps2; i += 1) {
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(right - i, hitbox.Y, 1, hitbox.Height), gradientB);
				}
			}

			if (modPlayer.StockSkill && modPlayer.StockMax > 1) {
				if (modPlayer.StockCount == 1) {
					spriteBatch.Draw(skillStock1, aboveHead, Color.White);
				}
				else if (modPlayer.StockCount == 2) {
					spriteBatch.Draw(skillStock2, aboveHead, Color.White);
				}
				else if (modPlayer.StockCount == 3) {
					spriteBatch.Draw(skillStock3, aboveHead, Color.White);
				}
			}
			else if (!modPlayer.StockSkill) {
				if (modPlayer.StockCount == 1) {
					spriteBatch.Draw(skill, aboveHead, Color.White);
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
