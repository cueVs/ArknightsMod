using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons;

public sealed class OperatorWeaponRecipeGroups : ModSystem
{
    public const string AnyVanillaPiano = "ArknightsMod:AnyVanillaPiano";

    public override void AddRecipeGroups()
    {
        // 按实际放置的家具类型收集原版钢琴，所有家具套装共用一个材料槽和一道配方。
        int[] pianos = ContentSamples.ItemsByType
            .Where(entry => entry.Key > 0 && entry.Key < ItemID.Count && entry.Value.createTile == TileID.Pianos)
            .Select(entry => entry.Key).OrderBy(type => type).Prepend(ItemID.Piano).Distinct().ToArray();
        RecipeGroup.RegisterGroup(AnyVanillaPiano, new RecipeGroup(
            () => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Piano), pianos));
    }
}
