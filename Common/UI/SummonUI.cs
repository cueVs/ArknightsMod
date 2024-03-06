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
	internal class SummonUI : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private UIElement button;

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.

			button = new UIElement();
			button.Left.Set(330, 0f);
			button.Top.Set(150, 0f);
			button.Width.Set(50, 0f);
			button.Height.Set(50, 0f);
			button.OnLeftClick += SummonMode;

			Append(button);
		}

		private void SummonMode(UIMouseEvent evt, UIElement listeningElement) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.ShowSummonIconBySkills[modPlayer.Skill]) {
				modPlayer.SummonMode = true;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// This prevents drawing unless we are using an ExampleCustomResourceWeapon
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.LocalPlayer.HeldItem.ModItem is PozemkaCrossbow) {
				base.Draw(spriteBatch);
			}
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			int skill = modPlayer.Skill + 1;

			if (modPlayer.ShowSummonIconBySkills[modPlayer.Skill]) {
				Texture2D skillBase = (Texture2D)ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SummonIcon/" + modPlayer.IconName + skill);
				spriteBatch.Draw(skillBase, new Vector2(330, 150), null, Color.White, 0f, Vector2.Zero, 1, 0, 1f);
			}

		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (Main.mouseX > 330 && Main.mouseX < 380 && Main.mouseY > 140 && Main.mouseY < 190) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	class SummonUISystem : ModSystem
	{
		private UserInterface SummonUserInterface;

		internal SummonUI _SummonUI;

		public void ShowMyUI() {
			SummonUserInterface?.SetState(_SummonUI);
		}
		public void HideMyUI() {
			SummonUserInterface?.SetState(null);
		}

		public override void Load() {
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
				_SummonUI = new();
				SummonUserInterface = new();
				SummonUserInterface.SetState(_SummonUI);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			SummonUserInterface?.Update(gameTime);
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
					"ArknightsMod: Summon Icon UI",
					delegate {
						SummonUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
