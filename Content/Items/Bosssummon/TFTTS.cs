using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.GameContent.ItemDropRules;
using ArknightsMod.Content.NPCs.Enemy.Seamonster;
using Terraria.Audio;

namespace ArknightsMod.Content.Items.Bosssummon
{
	public class TFTTS : ModItem
	{
		public override void AddRecipes() {
			
			CreateRecipe()
				.AddIngredient<Material.CoagulatingGel>(4)
				.AddIngredient<Material.CorruptedRecord>(20)
				.Register();
		}

	
	public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.maxStack = 1;
			Item.value = 1;
			Item.rare = 1;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
		}
		public override bool CanUseItem(Player player) {
			return
			!NPC.AnyNPCs(ModContent.NPCType<TheFirstToTalk>());
		}
		private int type;
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(SoundID.Waterfall, player.position);
				type = ModContent.NPCType<TheFirstToTalk>();
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.SpawnOnPlayer(player.whoAmI, type);
			}
			//else {
			//NetMessage.SendData(MessageID.SpawnBoss,)
			//}
			return true;
		}
	}
}
