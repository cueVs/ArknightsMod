using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
    public class VulnerableGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        //灼燃易伤，后续会改成减少法抗
        public int vulnerableTimer;

        public override void ResetEffects(NPC npc)
        {
            if (vulnerableTimer > 0)
                vulnerableTimer--;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (vulnerableTimer > 0)
                modifiers.FinalDamage *= 1.2f;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (vulnerableTimer > 0)
                modifiers.FinalDamage *= 1.2f;
        }
    }
}