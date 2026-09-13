using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.DisplayForUI
{
	public class Drone : ModItem
	{
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 2));
			ItemID.Sets.AnimatesAsSoul[Type] = true;
		}
	}
}
