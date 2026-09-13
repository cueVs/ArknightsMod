using Microsoft.Xna.Framework;
using Terraria;

namespace ArknightsMod.Content.ElementalImpairment.Effect
///————前言=————
///你可以在这里创建/设置一个全新的元素损伤类型

///————用法————
///创建一个类名
///重写参数（如果不写则以基类文件设置的默认值作为标准）
///MaxValue			最大损伤值
///BurstDamage		爆条伤害
///BurstDamageColor 伤害颜色
///IconColor		元素损伤图标颜色
///FeatherColor		元素损伤图标外层颜色
///FeatherScale		外层图标大小
///MainScale		元素损伤图标大小
///BurstFlashMainColor 爆条颜色（内层，默认白色，无需编写）
///BurstFlashFeatherColor 爆条外层颜色（与FeatherColor设置的一样就差不多了）
///
///OnBurstEffects(NPC npc)钩子
///在这里写上你想在爆条期间内给敌人/或者别的什么身上给予什么效果

///————如何给武器添加元素损伤效果————
///在你的弹幕的OnHitNPC或者ModifyHitNPC钩子里
///添加以下代码
///var container = target.GetGlobalNPC<AfflictionGlobalNPC>().Container;
///if (container == null) return;
///来获取对应的东西
///然后
///container.AddAfflictionValue<填写损伤类型>((int)(damageDone));
///务必引用using ArknightsMod.Content.ElementalImpairment.Effect;不然无法获取容器
///你可以随意设置数值，想让造成的损伤为伤害的15%？那就把damageDone改成damageDone * 0.15f
{
	public class BurnImpairment : ElementalAffliction
    {
        public override int MaxValue => 200;
        public override int BurstDamage => 1400;
        public override int CooldownTicks => 600;
        public override Color BurstDamageColor => Color.Red;
        public override Color IconColor => new Color(108, 16, 16); 
        public override Color FeatherColor => new Color(239, 74, 74); 
        public override float FeatherScale => 0.184f;
        public override float MainScale => 0.215f;
        public override Color BurstFlashMainColor => new Color(255, 255, 255);
        public override Color BurstFlashFeatherColor => new Color(239, 74, 74, 60);
        public override void OnBurstEffects(NPC npc)
        {
            var v = npc.GetGlobalNPC<VulnerableGlobalNPC>();
            v.vulnerableTimer = 600; 
            BurnFireEffect.Play(npc);     
        }
    }
    public class NervousImpairment : ElementalAffliction
    {
        public override int MaxValue => 200;
        public override int BurstDamage => 1200;
        public override int CooldownTicks => 600;
        public override Color BurstDamageColor => new Color(7, 35, 23);
        public override Color IconColor => new Color(7, 35, 23);
        public override Color FeatherColor => new Color(77, 126, 105);
        public override float FeatherScale => 0.184f;
        public override float MainScale => 0.215f;
        public override Color BurstFlashMainColor => new Color(255, 255, 255);
        public override Color BurstFlashFeatherColor => new Color(109, 146, 125, 60);
		public override void OnBurstEffects(NPC npc) {

			npc.GetGlobalNPC<PalsyGlobalNPC>().AddPalsyStacks(3);
		}
	}
    public class NecrosisImpairment : ElementalAffliction
    {
        public override int MaxValue => 200;
        public override int BurstDamage => 0;
        public override int CooldownTicks => 900;
        public override Color BurstDamageColor => new Color(0, 0, 0, 0);
        public override Color IconColor => new Color(46, 40, 50);
        public override Color FeatherColor => new Color(134, 126, 137);
        public override float FeatherScale => 0.184f;
        public override float MainScale => 0.215f;
        public override Color BurstFlashMainColor => new Color(255, 255, 255);
        public override Color BurstFlashFeatherColor => new Color(134, 126, 137, 60);
        public override void OnBurstEffects(NPC npc)
        {

            WeaknessSystem.ApplyWeakness(npc, 15f);
        }
    }
    public class CorrosionImpairment : ElementalAffliction
    {
        public override int MaxValue => 200;
        public override int BurstDamage => 1000;
        public override int CooldownTicks => 480;
        public override Color BurstDamageColor => new Color(99, 74, 100);

        public override Color IconColor => new Color(56, 30, 54);
        public override Color FeatherColor => new Color(99, 74, 100);
        public override float FeatherScale => 0.184f;
        public override float MainScale => 0.215f;
        public override Color BurstFlashMainColor => new Color(255, 255, 255);
        public override Color BurstFlashFeatherColor => new Color(99, 74, 100, 60);
        public override void OnBurstEffects(NPC npc)
        {
            ApplyDefenseReduction(npc, 45);
            
        }
    }
}