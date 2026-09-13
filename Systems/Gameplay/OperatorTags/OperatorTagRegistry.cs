using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor.Defender.Mudrock;
using ArknightsMod.Content.Items.Armor.Guard.Mlynar;
using ArknightsMod.Content.Items.Armor.Guard.Oblivionis;
using ArknightsMod.Content.Items.Armor.Guard.Skadi;
using ArknightsMod.Content.Items.Armor.Guard.Ulpianus;
using ArknightsMod.Content.Items.Armor.Specialist.Dorothy;
using ArknightsMod.Content.Items.Armor.Specialist.ExusiaiAlter;
using ArknightsMod.Content.Items.Armor.Sniper.Exusiai;
using ArknightsMod.Content.Items.Armor.Sniper.Fartooth;
using ArknightsMod.Content.Items.Armor.Sniper.Rosmontis;
using ArknightsMod.Content.Items.Armor.Caster.Mostima;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Armor.Supporter.CivilightEterna;
using ArknightsMod.Content.Items.Armor.Supporter.Ling;
using ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.OperatorTags
{
	internal static class OperatorTagRegistry
	{
		private static readonly Dictionary<int, OperatorTagEntry> ByHelmet = new();

		internal readonly struct OperatorTagEntry
		{
			public readonly OperatorClass Class;
			public readonly OperatorFaction Factions;

			public OperatorTagEntry(OperatorClass cls, OperatorFaction factions) {
				Class = cls;
				Factions = factions;
			}
		}

		internal static void Register(int helmetType, OperatorClass cls, OperatorFaction factions) {
			ByHelmet[helmetType] = new OperatorTagEntry(cls, factions);
		}

		// 注册时登记的是**时装**头部件的 ItemType，但调用方传进来的是 player.armor[0].type。
		// 对已迁移到 NeoArmor Reforge 的干员来说，穿在盔甲栏里的是自动生成的**套装件**，
		// 它是另一个 ItemID，直接查表必然落空（表现为干员标签、职业/阵营判定全部失效，
		// 且没有任何报错）。所以查不到时再用套装件反查它对应的时装，拿时装类型重试一次。
		// 这样处理是"未来自动生效"的：后续批次迁移任何干员都不需要再改这里。
		internal static bool TryGetFromHelmet(int helmetType, out OperatorTagEntry entry) {
			if (ByHelmet.TryGetValue(helmetType, out entry))
				return true;

			NeoArmorReforgeVanityItem vanity = NeoArmorReforgeSetLoader.GetVanity(helmetType);
			return vanity != null && ByHelmet.TryGetValue(vanity.Type, out entry);
		}

		internal static void Clear() => ByHelmet.Clear();
	}

	internal class OperatorTagLoader : ModSystem
	{
		public override void OnModLoad() {
			OperatorTagRegistry.Clear();
			RegisterAll();
		}

		private static void RegisterAll() {
			Register<MudrockHead>(OperatorClass.Defender, OperatorFaction.Sarkaz | OperatorFaction.RhodesIsland);
			Register<MlynarHead>(OperatorClass.Guard, OperatorFaction.Kazimierz);
			Register<OblivionisHead>(OperatorClass.Guard, OperatorFaction.AveMujica);
			Register<UlpianusHead>(OperatorClass.Guard, OperatorFaction.AbyssalHunter);
			Register<SkadiHead>(OperatorClass.Guard, OperatorFaction.AbyssalHunter);
			Register<RosmontisHead>(OperatorClass.Sniper, OperatorFaction.RhodesIsland);
			Register<DorothyHead>(OperatorClass.Specialist, OperatorFaction.RhodesIsland);
			Register<ExusiaiHead>(OperatorClass.Sniper, OperatorFaction.Laterano);
			Register<ExusiaiAlterHead>(OperatorClass.Specialist, OperatorFaction.Laterano);
			Register<MostimaHead>(OperatorClass.Caster, OperatorFaction.Laterano);
			Register<FartoothHead>(OperatorClass.Sniper, OperatorFaction.Laterano);
			Register<CivilightEternaHead>(OperatorClass.Supporter, OperatorFaction.Sarkaz | OperatorFaction.RhodesIsland);
			Register<LingHead>(OperatorClass.Supporter, OperatorFaction.RhodesIsland);
			Register<BagpipeHead>(OperatorClass.Vanguard, OperatorFaction.RhodesIsland);
		}

		private static void Register<THead>(OperatorClass cls, OperatorFaction factions)
			where THead : ModItem {
			OperatorTagRegistry.Register(ModContent.ItemType<THead>(), cls, factions);
		}
	}
}
