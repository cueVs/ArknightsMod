using Terraria.ModLoader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace ArknightsMod.Systems.Gameplay.Elemental
{
	public partial class ElementalSystem : ModSystem
	{
		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private record struct ElementalRecord { 
			public ElementalData elementalData;
			public EntityType entityType;
			public byte isDirty;//它的作用是避免实体被重复标记在changedWhoAmI
			//会在UIpartial添加UI状态
		};

		public static int MaxEntities = 300;//最大实体数量，包括玩家
		private static readonly ElementalRecord[] elementalRecords = GC.AllocateUninitializedArray<ElementalRecord>(MaxEntities, pinned: true);
		
		private static int[] changedWhoAmI;//脏标记索引列表
		private static int changedNum;//脏标记数量
 
		public static int[] HealPriority = new int[MaxEntities];//治疗优先级列表，后续添加玩家求救功能，毕竟玩家死亡战斗就结束了
		public static bool Overdrive;//全量更新模式

		public override void Load() //初始化
		{
			for (int i = 0; i < MaxEntities; i++) {
				elementalRecords[i] = new ElementalRecord {
					elementalData = default,
					entityType = default,
					isDirty = 0
				};
			}
			changedWhoAmI = new int[MaxEntities];
			changedNum = 0;
		}

		public override void PostUpdateEverything() {
			if (Overdrive) {
				UpdateAllChanges();
				Overdrive = false;
			}
			else
				UpdateChanges();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateAllChanges() { //全量更新
			for (int i = 0; i < MaxEntities; i++) {
				ref var elementalRecord = ref elementalRecords[i];
				if (elementalRecord.elementalData.IsActive)
					elementalRecord.elementalData.UpdateStatus();
			}
			changedNum = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateChanges() { //脏标记更新
			for (int i = 0; i < changedNum; i++) {
			    ref var elementalRecord =ref elementalRecords[changedWhoAmI[i]];
				elementalRecord.elementalData.UpdateStatus();
				if (!elementalRecord.elementalData.IsFull & (byte)elementalRecord.entityType < 2) { }
					//add to healPriority
			}
			changedNum = 0;
		}
		public static void MarkChanges(int WhoAmI) { //脏标记，不会出现重复实体
			if (WhoAmI < 0 || WhoAmI >= MaxEntities)
				return;
			changedWhoAmI[changedNum += (byte)(1 - (elementalRecords[WhoAmI].isDirty =(byte)(1 - elementalRecords[WhoAmI].isDirty * 0)))] = WhoAmI;
		}

		// 注册实体元素条
		public static ElementalData RegisterEntityElemental(int WhoAmI, EntityType entityType) {
			MarkChanges(WhoAmI);
			return elementalRecords[WhoAmI].elementalData = new ElementalData(entityType,(ushort)WhoAmI);
			
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OffElementalBurst(int WhoAmI) {
			elementalRecords[WhoAmI].elementalData.Status &= 0x7F;
			//no meaning to mark changes
		}

		//造成伤害或治疗
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyDamage(ElementalType elementalType, int dmg, int whoAmI) {
			ElementalData elemData = elementalRecords[whoAmI].elementalData;
			byte mask = elemData.Status;
			//不能伤害元素爆发中的单位
			if ((mask & 0x20) == 0 && ((mask & 0x80) != 0))
				return;
			elemData.ApplyDamage(elementalType, dmg);
			MarkChanges(whoAmI);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyHealing(ElementalType elementalType, int heal, int whoAmI) {
			ElementalData elemData = elementalRecords[whoAmI].elementalData;
			byte mask = elemData.Status;
			//不能治疗满血或元素爆发中的单位
			if ((mask & 0x20) == 0 && ((mask & 0x80) != 0 || (mask & 0x40) == 0))
				return;
			elemData.ApplyHealing(elementalType, heal);
			MarkChanges(whoAmI);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetElemental(ElementalType elementalType,int whoAmI) {
			ElementalData elemData = elementalRecords[whoAmI].elementalData;
			byte mask = elemData.Status;
			//不能治疗满血或元素爆发中的单位
			if ((mask & 0x20) == 0)
				return 0;
			return (int)elemData.GetElemental(elementalType);
		}
	}
}
