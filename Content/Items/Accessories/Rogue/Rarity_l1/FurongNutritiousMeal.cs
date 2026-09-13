using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1
{
    public class FurongNutritiousMeal : ModItem
    {
       //减少5%伤害（怪物）

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
            player.GetModPlayer<FurongNutritiousMealPlayer>().hasEmblem = true;
        }

        
    }

    public class FurongNutritiousMealPlayer : ModPlayer
    {
        public bool hasEmblem;

        public override void ResetEffects()
        {
            hasEmblem = false;
        }

        // 减少所有NPC对玩家的伤害
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (hasEmblem && npc != null && !npc.friendly)
            {

                modifiers.SourceDamage *= 0.95f;


                
            }
        }

        // 减少所有NPC弹幕对玩家的伤害
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (hasEmblem && proj != null && proj.hostile && proj.npcProj)
            {

                modifiers.SourceDamage *= 0.95f;


                
            }
        }
    }
}