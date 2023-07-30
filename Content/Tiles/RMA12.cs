using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ArknightsMod.Content.Tiles
{
	public class RMA12 : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileID.Sets.Ore[Type] = true;
			// Main.tileSpelunker[Type] = true; // The tile will be affected by spelunker highlighting
			Main.tileOreFinderPriority[Type] = 180; // Metal Detector value, see https://terraria.gamepedia.com/Metal_Detector
													// Main.tileShine2[Type] = true; // Modifies the draw color slightly.
			Main.tileShine[Type] = 975; // How often tiny dust appear off this tile. Larger is less frequently. Example: 975
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type][TileID.WoodBlock] = true;
			// Main.tileMerge[Type][TileID.Stone] = true; //If you enable this line, you must also enable the same option of (Global)Stone, because the Stones don't merge any blocks.
			Main.tileMerge[Type][TileID.SnowBlock] = true;
			Main.tileMerge[Type][TileID.IceBlock] = true;
			Main.tileMerge[Type][TileID.Sand] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Mods.ArknightsMod.Tile.OrirockCube}");
			AddMapEntry(new Color(245, 238, 197), name); // Adds an entry to the minimap for this tile with the given color and display name. This should be called in SetDefaults. 

			DustType = DustID.Bone;
			HitSound = SoundID.Tink;
			// MineResist = 4f;
			MinPick = 30;
		}
	}

	public class RMA12System : ModSystem
	{
		public static LocalizedText RMA12PassMessage { get; private set; }

		public override void SetStaticDefaults() {
			RMA12PassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(RMA12PassMessage)}"));
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			// Because world generation is like layering several images ontop of each other, we need to do some steps between the original world generation steps.

			// The first step is an Ore. Most vanilla ores are generated in a step called "Shinies", so for maximum compatibility, we will also do this.
			// First, we find out which step "Shinies" is.
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));

			if (ShiniesIndex != -1)
			{
				// Next, we insert our pass directly after the original "Shinies" pass.
				// ExampleOrePass is a class seen bellow
				tasks.Insert(ShiniesIndex + 1, new RMA12Pass("RMA70-12", 237.4298f));
			}
		}
	}

	public class RMA12Pass : GenPass
	{
		public RMA12Pass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			// progress.Message is the message shown to the user while the following code is running.
			// Try to make your message clear. You can be a little bit clever, but make sure it is descriptive enough for troubleshooting purposes.
			progress.Message = "RMA70-12";

			// Ores are quite simple, we simply use a for loop and the WorldGen.TileRunner to place splotches of the specified Tile in the world.
			// "6E-05" is "scientific notation". It simply means 0.00006 but in some ways is easier to read.
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-04); k++)
			{
				// The inside of this for loop corresponds to one single splotch of our Ore.
				// First, we randomly choose any coordinate in the world by choosing a random x and y value.
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				// WorldGen.worldSurfaceLow is actually the highest surface tile. In practice you might want to use WorldGen.rockLayer or other WorldGen values.
				int y = WorldGen.genRand.Next((int)GenVars.rockLayer, Main.maxTilesY);

				// Then, we call WorldGen.TileRunner with random "strength" and random "steps", as well as the Tile we wish to place.
				// Feel free to experiment with strength and step to see the shape they generate.
				// Alternately, we could check the tile already present in the coordinate we are interested.
				// Wrapping WorldGen.TileRunner in the following condition would make the ore only generate in Dirt.
				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.HasTile && tile.TileType == TileID.Dirt)
				{
					WorldGen.TileRunner(x, y, WorldGen.genRand.Next(4, 6), WorldGen.genRand.Next(5, 8), ModContent.TileType<RMA12>());
				}
			}
		}
	}
}
