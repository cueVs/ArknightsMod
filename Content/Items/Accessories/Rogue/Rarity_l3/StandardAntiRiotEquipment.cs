using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class StandardAntiRiotEquipment : ModItem
    {
       

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ArcaneAmplifierPlayer3>().damageAmplifierActive = true;
        }
    }

    public class ArcaneAmplifierPlayer3 : ModPlayer
    {
        public bool damageAmplifierActive;

        public override void ResetEffects()
        {
            damageAmplifierActive = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!damageAmplifierActive) return;


            if (item.DamageType == DamageClass.Magic || item.DamageType == DamageClass.Summon)
            {
                modifiers.FinalDamage *= 1.20f;
            }
        }
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (!damageAmplifierActive)
				return;

			// ºÏ≤‚∑® ıªÚ’ŸªΩ…À∫¶
			if (proj.DamageType == DamageClass.Magic || proj.DamageType == DamageClass.Summon) {
				modifiers.FinalDamage *= 1.20f;
			}
		}

	}
}