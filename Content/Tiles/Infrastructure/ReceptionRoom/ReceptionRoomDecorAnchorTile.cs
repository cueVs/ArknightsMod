using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	public class ReceptionRoomDecorAnchorTile : ModTile
	{
		public override string Texture => global::ArknightsMod.ArknightsMod.noTexture;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileNoFail[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(0, 0, 0, 0));
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			type = -1;
			return false;
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => true;

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;
			noItem = true;
			ReceptionRoomDecorSystem.TryRemoveTopMostAt(i, j);
		}

		public override bool RightClick(int i, int j)
		{
			return ReceptionRoomDecorSystem.TryRightClick(i, j, Main.LocalPlayer);
		}

		public override void MouseOver(int i, int j)
		{
			ReceptionRoomDecorSystem.TryMouseOver(i, j, Main.LocalPlayer);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;
	}
}
