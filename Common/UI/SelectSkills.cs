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
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameInput;

namespace ArknightsMod.Common.UI
{
	internal class SelectSkills : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private UIText initialSP1;
		private UIText initialSP2;
		private UIText initialSP3;
		private UIText maxSP1;
		private UIText maxSP2;
		private UIText maxSP3;
		private UIText skillLevel;
		private UIElement area;
		private UIElement buttonS1;
		private UIElement buttonS2;
		private UIElement buttonS3;

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.
			area = new UIElement();
			area.Left.Set(10, 0f); // Place the resource bar to the left of the hearts.
			area.Top.Set(100, 0f); // Placing it just a bit below the top of the screen.
			area.Width.Set(300, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
			area.Height.Set(100, 0f);
			//area.VAlign = 0.5f;
			//area.HAlign = 0.5f;

			buttonS1 = new UIElement();
			buttonS1.Left.Set(16, 0f);
			buttonS1.Top.Set(16, 0f);
			buttonS1.Width.Set(64, 0f);
			buttonS1.Height.Set(64, 0f);
			buttonS1.OnLeftClick += ChangeS1;

			initialSP1 = new UIText("0", 0.8f); // text to show stat
			initialSP1.Left.Set(28, 0f);
			initialSP1.Top.Set(68, 0f);
			initialSP1.Width.Set(20, 0f);
			initialSP1.Height.Set(12, 0f);

			maxSP1 = new UIText("0", 0.8f); // text to show stat
			maxSP1.Left.Set(58, 0f);
			maxSP1.Top.Set(68, 0f);
			maxSP1.Width.Set(20, 0f);
			maxSP1.Height.Set(12, 0f);

			buttonS2 = new UIElement();
			buttonS2.Left.Set(86, 0f);
			buttonS2.Top.Set(16, 0f);
			buttonS2.Width.Set(64, 0f);
			buttonS2.Height.Set(64, 0f);
			buttonS2.OnLeftClick += ChangeS2;

			initialSP2 = new UIText("0", 0.8f); // text to show stat
			initialSP2.Left.Set(100, 0f);
			initialSP2.Top.Set(68, 0f);
			initialSP2.Width.Set(20, 0f);
			initialSP2.Height.Set(12, 0f);

			maxSP2 = new UIText("0", 0.8f); // text to show stat
			maxSP2.Left.Set(130, 0f);
			maxSP2.Top.Set(68, 0f);
			maxSP2.Width.Set(20, 0f);
			maxSP2.Height.Set(12, 0f);

			buttonS3 = new UIElement();
			buttonS3.Left.Set(156, 0f);
			buttonS3.Top.Set(16, 0f);
			buttonS3.Width.Set(64, 0f);
			buttonS3.Height.Set(64, 0f);
			buttonS3.OnLeftClick += ChangeS3;

			initialSP3 = new UIText("0", 0.8f); // text to show stat
			initialSP3.Left.Set(170, 0f);
			initialSP3.Top.Set(68, 0f);
			initialSP3.Width.Set(20, 0f);
			initialSP3.Height.Set(12, 0f);

			maxSP3 = new UIText("0", 0.8f); // text to show stat
			maxSP3.Left.Set(200, 0f);
			maxSP3.Top.Set(68, 0f);
			maxSP3.Width.Set(20, 0f);
			maxSP3.Height.Set(12, 0f);

			skillLevel = new UIText("0", 1f); // text to show stat
			skillLevel.Left.Set(272, 0f);
			skillLevel.Top.Set(34, 0f);
			skillLevel.Width.Set(20, 0f);
			skillLevel.Height.Set(20, 0f);

			area.Append(buttonS1);
			area.Append(buttonS2);
			area.Append(buttonS3);
			area.Append(initialSP1);
			area.Append(initialSP2);
			area.Append(initialSP3);
			area.Append(maxSP1);
			area.Append(maxSP2);
			area.Append(maxSP3);
			area.Append(skillLevel);
			Append(area);
		}

		private void ChangeS1(UIMouseEvent evt, UIElement listeningElement) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.HowManySkills > 0 && modPlayer.Skill != 0) {
				modPlayer.Skill = 0;
				modPlayer.SkillInitialize = true;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/UIButton"));
			}
		}

		private void ChangeS2(UIMouseEvent evt, UIElement listeningElement) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.HowManySkills > 1 && modPlayer.Skill != 1) {
				modPlayer.Skill = 1;
				modPlayer.SkillInitialize = true;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/UIButton"));
			}
		}

		private void ChangeS3(UIMouseEvent evt, UIElement listeningElement) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.HowManySkills > 2 && modPlayer.Skill != 2) {
				modPlayer.Skill = 2;
				modPlayer.SkillInitialize = true;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/UIButton"));
			}
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
			if (Main.LocalPlayer.HeldItem.ModItem is PozemkaCrossbow) {
				base.Draw(spriteBatch);
			}
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			Texture2D skillBase = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillBase");
			spriteBatch.Draw(skillBase, new Vector2(20, 126), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);

			// S1
			if (modPlayer.HowManySkills > 0) {
				Texture2D s1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/" + modPlayer.SkillIconName + "1");
				spriteBatch.Draw(s1, new Vector2(26, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				Texture2D baseOfSPs1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BaseOfSP");
				spriteBatch.Draw(baseOfSPs1, new Vector2(32, 168), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				if (modPlayer.MasteryLevelS1 == 1) {
					Texture2D masteryLevelS1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M1");
					spriteBatch.Draw(masteryLevelS1, new Vector2(22, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS1 == 2) {
					Texture2D masteryLevelS1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M2");
					spriteBatch.Draw(masteryLevelS1, new Vector2(22, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS1 == 3) {
					Texture2D masteryLevelS1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M3");
					spriteBatch.Draw(masteryLevelS1, new Vector2(22, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
			}
			else {
				Texture2D noS1 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/NoSkill");
				spriteBatch.Draw(noS1, new Vector2(26, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
			}

			// S2
			if (modPlayer.HowManySkills > 1) {
				Texture2D s2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/" + modPlayer.SkillIconName + "2");
				spriteBatch.Draw(s2, new Vector2(96, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				Texture2D baseOfSPs2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BaseOfSP");
				spriteBatch.Draw(baseOfSPs2, new Vector2(102, 168), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				if (modPlayer.MasteryLevelS2 == 1) {
					Texture2D masteryLevelS2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M1");
					spriteBatch.Draw(masteryLevelS2, new Vector2(92, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS2 == 2) {
					Texture2D masteryLevelS2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M2");
					spriteBatch.Draw(masteryLevelS2, new Vector2(92, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS2 == 3) {
					Texture2D masteryLevelS2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M3");
					spriteBatch.Draw(masteryLevelS2, new Vector2(92, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
			}
			else {
				Texture2D noS2 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/NoSkill");
				spriteBatch.Draw(noS2, new Vector2(96, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
			}

			// S3
			if (modPlayer.HowManySkills > 2) {
				Texture2D s3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/" + modPlayer.SkillIconName + "3");
				spriteBatch.Draw(s3, new Vector2(166, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				Texture2D baseOfSPs3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BaseOfSP");
				spriteBatch.Draw(baseOfSPs3, new Vector2(172, 168), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				if (modPlayer.MasteryLevelS3 == 1) {
					Texture2D masteryLevelS3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M1");
					spriteBatch.Draw(masteryLevelS3, new Vector2(162, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS3 == 2) {
					Texture2D masteryLevelS3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M2");
					spriteBatch.Draw(masteryLevelS3, new Vector2(162, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
				else if (modPlayer.MasteryLevelS3 == 3) {
					Texture2D masteryLevelS3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/M3");
					spriteBatch.Draw(masteryLevelS3, new Vector2(162, 110), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
				}
			}
			else {
				Texture2D noS3 = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillIcons/NoSkill");
				spriteBatch.Draw(noS3, new Vector2(164, 116), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
			}

		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			// Setting the text per tick to update and show our resource values.
			initialSP1.SetText($"{modPlayer.InitialSPs1}");
			initialSP2.SetText($"{modPlayer.InitialSPs2}");
			initialSP3.SetText($"{modPlayer.InitialSPs3}");
			maxSP1.SetText($"{modPlayer.MaxSPs1}");
			maxSP2.SetText($"{modPlayer.MaxSPs2}");
			maxSP3.SetText($"{modPlayer.MaxSPs3}");
			skillLevel.SetText($"{modPlayer.SkillLevel}");

			if(Main.mouseX > 26 && Main.mouseX < 230 && Main.mouseY > 116 && Main.mouseY < 180) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	class SelectSkillsSystem : ModSystem
	{
		private UserInterface SelectSkillsUserInterface;

		internal SelectSkills SelectSkillsUI;

		public void ShowMyUI() {
			SelectSkillsUserInterface?.SetState(SelectSkillsUI);
		}
		public void HideMyUI() {
			SelectSkillsUserInterface?.SetState(null);
		}

		public override void Load() {
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
				SelectSkillsUI = new();
				SelectSkillsUserInterface = new();
				SelectSkillsUserInterface.SetState(SelectSkillsUI);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			SelectSkillsUserInterface?.Update(gameTime);
			if (PlayerInput.Triggers.JustPressed.Inventory) {
				if (Main.playerInventory) {
					HideMyUI();
				}
				else {
					ShowMyUI();
				}
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars")) + 1;
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"ArknightsMod: Skill Select",
					delegate {
						SelectSkillsUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
