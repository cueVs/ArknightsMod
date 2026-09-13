using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;

namespace ArknightsMod.Content.NPCs.Enemy.Evolution.Evoution_skill
{
	public class Evolution_skill{
		public void Ring_Attack(int damage) {
			if(Main.netMode == NetmodeID.MultiplayerClient)
				return;
		
		}
	}

}