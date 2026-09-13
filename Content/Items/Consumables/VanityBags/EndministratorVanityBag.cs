using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.Endfield.Guard.Endministrator;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Consumables.VanityBags
{
	public class EndministratorVanityBag : ArknightsVanityBag
	{
		public override string Texture => "ArknightsMod/Content/Items/Armor/Endfield/Guard/Endministrator/EndminFemaleDefault";

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.UseSound = SoundID.Item4;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return true;

			foreach (int itemType in GetItems())
				player.QuickSpawnItem(player.GetSource_OpenItem(Type), itemType, 1);

			return true;
		}

		public override ObtainTypes ObtainType => ObtainTypes.EndfieldDefault;
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<EndminFemaleHead>(),
				ModContent.ItemType<EndminFemaleBody>(),
				ModContent.ItemType<EndminFemaleLegs>(),
				ModContent.ItemType<EndminMask>(),
			];
		}
	}
}
