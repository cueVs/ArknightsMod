using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
    public class CrackedRestraintStrap : ModItem
    {
       

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = 1;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CrackedRestraintStrapPlayer>().hasEmblem = true;
        }

        
    }

    public class CrackedRestraintStrapPlayer : ModPlayer
    {
        public bool hasEmblem;

        public override void ResetEffects()
        {
            hasEmblem = false;
        }


        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (hasEmblem && npc != null && !npc.friendly)
            {
                // ºı…Ÿ17%…À∫¶£®≥ÀÀ„£©
                modifiers.SourceDamage *= 0.93f;


                
            }
        }


        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (hasEmblem && proj != null && proj.hostile && proj.npcProj)
            {

                modifiers.SourceDamage *= 0.93f;


                
            }
        }
    }
}