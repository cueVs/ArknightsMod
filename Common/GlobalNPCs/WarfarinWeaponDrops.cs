using ArknightsMod.Content.Items.Weapons.Medic.Warfarin;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs;

public sealed class WarfarinWeaponDrops : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type is NPCID.ZombieMerman or NPCID.EyeballFlyingFish)
            npcLoot.Add(new CommonDrop(ModContent.ItemType<WarfarinStaff>(), 100, 1, 1, 33));
    }
}
