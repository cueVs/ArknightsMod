using Terraria;
using ArknightsMod.Common.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;

public class ElementalUI : ModSystem
{
	internal Santable santable;
	internal UserInterface sanUserInterface;
	public override void Load() {
		santable = new Santable();
		santable.Activate();
		sanUserInterface = new UserInterface();
		sanUserInterface.SetState(santable);
	}
	public override void UpdateUI(GameTime gameTime) {
		if (Santable.Visible) {
			sanUserInterface?.Update(gameTime);
		}
		base.UpdateUI(gameTime);
	}
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (MouseTextIndex != -1) {
			layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
				"ArknightsMod : Santable",
				delegate {
					if (Santable.Visible)
						santable.Draw(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
		base.ModifyInterfaceLayers(layers);
	}

}