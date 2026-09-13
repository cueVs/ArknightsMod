using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1
{
    public class ActiveDust : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ActiveDustArcaneAmplifierPlayer>().damageAmplifierActive = true;
        }
    }

    public class ActiveDustArcaneAmplifierPlayer : ModPlayer
    {
        public bool damageAmplifierActive;

        public override void ResetEffects()
        {
            damageAmplifierActive = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
			if (!damageAmplifierActive)
				return;

			// ºÏ≤‚∑® ıªÚ’ŸªΩ…À∫¶
			if (item.DamageType == DamageClass.Magic || item.DamageType == DamageClass.Summon) {
				modifiers.FinalDamage *= 1.10f;
			}
		}

       
    }
}