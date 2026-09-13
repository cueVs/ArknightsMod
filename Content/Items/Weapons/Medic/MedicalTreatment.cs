using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic;

/// <summary>医疗武器共用的治疗结算：首次治疗生效，随后以抗药病阻止高频重复治疗。</summary>
public sealed class MedicalTreatmentPlayer : ModPlayer
{
    private int medicationSicknessTime;
    private float medicationReduction;

    internal int ApplyMedicalTreatment(int requestedAmount, int attackIntervalTicks, float reduction = 1f)
    {
        if (requestedAmount <= 0 || Player.dead || !Player.active)
            return 0;

        int amount = Math.Max(0, (int)MathF.Floor(requestedAmount * (1f - medicationReduction)));
        medicationSicknessTime = Math.Max(medicationSicknessTime, Math.Max(1, attackIntervalTicks * 2));
        medicationReduction = Math.Max(medicationReduction, Math.Clamp(reduction, 0f, 1f));
        return amount;
    }

    public override void PostUpdate()
    {
        if (medicationSicknessTime <= 0)
            return;
        if (--medicationSicknessTime == 0)
            medicationReduction = 0f;
    }

    public override void UpdateDead()
    {
        medicationSicknessTime = 0;
        medicationReduction = 0f;
    }
}

internal static class MedicalTreatment
{
    // 医疗干员基础攻击间隔 2.85 秒；按医疗五级规则换算为两倍。
    internal const int StandardAttackIntervalTicks = 342;
    internal const int InitialTreatmentDelayTicks = 30;

    internal static int Apply(Player target, int requestedAmount, int attackIntervalTicks = StandardAttackIntervalTicks,
        float medicationReduction = 1f)
    {
        if (target.statLife >= target.statLifeMax2)
            return 0;
        int amount = target.GetModPlayer<MedicalTreatmentPlayer>()
            .ApplyMedicalTreatment(requestedAmount, attackIntervalTicks, medicationReduction);
        return Math.Min(amount, Math.Max(0, target.statLifeMax2 - target.statLife));
    }
}
