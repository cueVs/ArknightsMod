using ArknightsMod.Content.Items.BattleRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ArknightsMod.Common.UI.BattleRecord
{
	public sealed class BattleRecordCalculator
	{
		private readonly Dictionary<Type, int> _battleRecordCount = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> _battleRecordBudget = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> _battleRecordExperience = new Dictionary<Type, int>();
		public int SumExperience { get; private set; }
		public int BuygetExperience { get; private set; }

		public int this[Type type, bool budget = false] {
			get {
				if (budget) {
					if (_battleRecordBudget.TryGetValue(type, out int v))
						return v;
					else
						return 0;
				}
				else {
					if (_battleRecordCount.TryGetValue(type, out int v))
						return v;
					else
						return 0;
				}
			}
			set {
				if (budget) {
					value = Math.Clamp(value, 0, this[type]);
					if (!_battleRecordBudget.TryAdd(type, value))
						_battleRecordBudget[type] = value;
				}
				else {
					if (!_battleRecordCount.TryAdd(type, value))
						_battleRecordCount[type] = value;
				}
			}
		}

		public void RefreshCount() {
			SumExperience = 0;
			_battleRecordCount.Clear();
			foreach (var item in Main.LocalPlayer.inventory) {
				if (item != null && item.ModItem != null && item.ModItem is BattleRecordBase battleRecord) {
					var type = battleRecord.GetType();
					_battleRecordExperience.TryAdd(type, battleRecord.Experience);
					this[type] += item.stack;
					SumExperience += item.stack * battleRecord.Experience;
				}
			}
			RefresBudget();
		}

		public void RefresBudget() {
			BuygetExperience = 0;
			var types = _battleRecordExperience.Keys.ToList();
			foreach (var item in _battleRecordCount) {
				if (this[item.Key, true] > item.Value) {
					this[item.Key, true] = item.Value;
				}
				types.Remove(item.Key);
				BuygetExperience += this[item.Key, true] * _battleRecordExperience[item.Key];
			}
			types.ForEach(t => _battleRecordBudget.Remove(t));
		}

		public void ClearBuyget() => _battleRecordBudget.Clear();

		public void Fulfilled() {
			foreach (var item in Main.LocalPlayer.inventory) {
				if (item != null && item.ModItem != null && item.ModItem is BattleRecordBase battleRecord) {
					var type = battleRecord.GetType();
					int budget = this[type, true];
					if (budget > 0) {
						item.stack -= budget;
						this[type, true] -= item.stack;

						if (item.stack <= 0)
							item.TurnToAir();
					}
				}
			}
			_battleRecordBudget.Clear();
			BuygetExperience = 0;
		}

		public void PlanBuyget(int targetExperience, bool round = true) {
			int expOffset = targetExperience - BuygetExperience;
			if (_battleRecordExperience.Count == 0 || expOffset == 0)
				return;
			var exps = _battleRecordExperience.ToList();
			exps.Sort((b1, b2) => b2.Value.CompareTo(b1.Value));
			int index = 0;
			if (expOffset > 0) {
				while (expOffset > 0) {
					var bct = exps[index];
					int count = this[bct.Key] - this[bct.Key, true];
					if (expOffset >= count * bct.Value) {
						expOffset -= count * bct.Value;
						this[bct.Key, true] += count;
					}
					else {
						count = expOffset / bct.Value;
						expOffset -= count * bct.Value;
						this[bct.Key, true] += count;
					}
					index++;
					if (index == exps.Count && expOffset > 0) {
						do {
							index--;
							bct = exps[index];
							count = this[bct.Key] - this[bct.Key, true];
							if (count == 0)
								continue;
							expOffset -= bct.Value;
							this[bct.Key, true]++;
						} while (expOffset > 0 && index > 0);
						break;
					}
				}
			}
			else {
				expOffset *= -1;
				bool needMakeUp = true;
				while (expOffset > 0) {
					var bct = exps[index];
					int count = this[bct.Key, true];
					if (count == 0) {
						index++;
						if (index == exps.Count)
							break;
						continue;
					}
					if (expOffset >= count * bct.Value) {
						expOffset -= count * bct.Value;
						this[bct.Key, true] = 0;
						needMakeUp = false;
					}
					else {
						count = expOffset / bct.Value;
						expOffset -= count * bct.Value;
						this[bct.Key, true] -= count;
					}
					if (needMakeUp)
						needMakeUp = count == 0;
					index++;
					if (index == exps.Count)
						break;
				}
				if (needMakeUp && round) {
					do {
						index--;
						var bct = exps[index];
						if (this[bct.Key, true] > 0) {
							this[bct.Key, true]--;
							break;
						}
					} while (index > 0);
				}
			}
			RefresBudget();
		}
	}
}