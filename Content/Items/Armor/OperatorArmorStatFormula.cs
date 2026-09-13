using System;

namespace ArknightsMod.Content.Items.Armor
{
	// NeoArmor 三件套数值换算公式：填入 PRTS 上干员"对应时期"(一般取精二满级)的生命/防御原始数值，
	// 自动算出头/躯干/腿三部位各自应有的 ArmorLifeBonus 与 Item.defense，写新装备时直接调用即可。
	// 换算规则：生命基数 = 生命 ÷ 5，防御基数 = 防御 ÷ 10；
	// 生命按 头50% / 躯干25% / 腿25% 分配，防御按 头0% / 躯干75% / 腿25% 分配。
	public static class OperatorArmorStatFormula
	{
		private const float LifeDivisor = 5f;
		private const float DefenseDivisor = 10f;

		private const float HeadLifeRatio = 0.5f;
		private const float BodyLifeRatio = 0.25f;
		private const float LegsLifeRatio = 0.25f;

		private const float HeadDefenseRatio = 0f;
		private const float BodyDefenseRatio = 0.75f;
		private const float LegsDefenseRatio = 0.25f;

		public static int HeadLifeBonus(int prtsLife) => (int)Math.Round(prtsLife / LifeDivisor * HeadLifeRatio);
		public static int BodyLifeBonus(int prtsLife) => (int)Math.Round(prtsLife / LifeDivisor * BodyLifeRatio);
		public static int LegsLifeBonus(int prtsLife) => (int)Math.Round(prtsLife / LifeDivisor * LegsLifeRatio);

		public static int HeadDefenseBonus(int prtsDefense) => (int)Math.Round(prtsDefense / DefenseDivisor * HeadDefenseRatio);
		public static int BodyDefenseBonus(int prtsDefense) => (int)Math.Round(prtsDefense / DefenseDivisor * BodyDefenseRatio);
		public static int LegsDefenseBonus(int prtsDefense) => (int)Math.Round(prtsDefense / DefenseDivisor * LegsDefenseRatio);
	}
}
