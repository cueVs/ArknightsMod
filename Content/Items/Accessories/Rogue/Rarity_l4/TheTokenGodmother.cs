using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class TheTokenGodmother : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<TheTokenGodmotherPlayer>().hasEmblem = true;
        }

        
    }

    public class TheTokenGodmotherPlayer : ModPlayer
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

                modifiers.SourceDamage *= 0.83f;


                
            }
        }


        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (hasEmblem && proj != null && proj.hostile && proj.npcProj)
            {

                modifiers.SourceDamage *= 0.83f;


                
            }
        }
    }
}