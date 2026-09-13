using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
    public class RustBlades : ModItem
    {
      

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = 1; // 浅紫色稀有度
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<VulnerabilityPlayer3>().damageAmplifierActive = true;
        }
    }

    public class VulnerabilityPlayer3 : ModPlayer
    {
        public bool damageAmplifierActive;

        public override void ResetEffects()
        {
            damageAmplifierActive = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
             if (!damageAmplifierActive) return;

            if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.Ranged)
            {
                modifiers.FinalDamage *= 1.15f;
            }
        }
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (!damageAmplifierActive)
				return;

			// 检测法术或召唤伤害
			if (proj.DamageType == DamageClass.Melee || proj.DamageType == DamageClass.Ranged) {
				modifiers.FinalDamage *= 1.15f;
			}
		}

	}
}