using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;

namespace ArknightsMod.Content.Items.Summon
{
    public class Spicycandy : ModItem
    {
        public override void SetStaticDefaults()
        {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // This helps sort inventory know that this is a boss summoning Item.
		}

		public override void SetDefaults() {
			Item.UseSound = SoundID.Item4;
			Item.useStyle = 4;
			Item.useTurn = false;
			Item.useAnimation = 5;
			Item.useTime = 5;
			Item.maxStack = 1;
			Item.consumable = false;
			Item.value = Item.buyPrice(0, 1);
			Item.width = 11;
			Item.height = 11;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = 1;
		}

		public override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return true;
			}

			SoundEngine.PlaySound(SoundID.Roar, player.position);//Play roar sound effect
			int type = ModContent.NPCType<FrostNova>();
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.SpawnOnPlayer(player.whoAmI, type);//Spawn Boss
			}
			else {
				NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);//Used for online synchronization
			}

			return true;
		}

		public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<FrostNova>());//The boss can only be summoned when it does not exist in the world.
        }

        public override void AddRecipes()
		{
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SpicyPepper, 1);
            recipe1.AddIngredient(ItemID.Shiverthorn, 1);
            recipe1.AddIngredient(ItemID.Ale, 1);
            recipe1.Register();
        }
    }
}
