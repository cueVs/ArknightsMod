using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace ArknightsMod.Systems
{
	public sealed unsafe class MegaTimer : ModSystem
	{
		[StructLayout(LayoutKind.Explicit, Size = 24)]
		private struct Timer
		{
			[FieldOffset(0)] public int TimerId;
			[FieldOffset(4)] public int WhoAmI;
			[FieldOffset(8)] public int EndTick;
			[FieldOffset(12)] public byte Level;
			[FieldOffset(13)] public byte TypeFlag;
			[FieldOffset(14)] public short WheelSlot;
			[FieldOffset(16)] public IntPtr Callback;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Wheel
		{
			public int* TimerSlots;
			public int CurrentTick;
		}

		private static Timer* _timer;
		private static Wheel* _wheel;

		private const int _wheelSize = 256;
		private const int _wheelBits = 8;
		private const int _wheelLv = 3;
		private const int _wheelMask = (1 << _wheelBits) - 1;
		private const int _timersPerBufferPage = 1 << 20;

		private static int* _freePage;
		private static int _freeNum;
		private static ILHook _updateTickHook;
		private static int _capacity;

		private static readonly Vector128<int> _allOneVector128 = Vector128.Create(-1);

		public override void OnModLoad() {
			_wheel = (Wheel*)NativeMemory.Alloc((nuint)(_wheelLv * sizeof(Wheel)));
			Unsafe.InitBlockUnaligned(_wheel, 0, (uint)(_wheelLv * sizeof(Wheel)));
			for (int i = 0; i < _wheelLv; i++) {
				_wheel[i].TimerSlots = (int*)NativeMemory.Alloc((nuint)(_wheelSize * sizeof(int)));
				Unsafe.InitBlockUnaligned(_wheel[i].TimerSlots, 0, (uint)(_wheelSize * sizeof(int)));
			}
			_capacity = _timersPerBufferPage;
			_timer = (Timer*)NativeMemory.Alloc((nuint)(_capacity * sizeof(Timer)));
			_freePage = (int*)NativeMemory.Alloc((nuint)(_capacity * sizeof(int)));

			for (int i = 0; i < _capacity; i++) {
				_freePage[i] = i;
			}
			_freeNum = _capacity;
			var updateTick = typeof(Main).GetMethod("Update",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null, [typeof(GameTime)], null);
			_updateTickHook = new ILHook(updateTick, UpdateILInjection);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AddTimer(int whoAmI, delegate*<int, void> callback, int inFrames, bool isOnEntity) {
			if (_freeNum <= 0) {
				ExpandCapacityByPage();
			}
			int timerId = _freePage[--_freeNum];

			ref var newTimer = ref _timer[timerId];
			newTimer.TimerId = timerId;
			newTimer.WhoAmI = whoAmI;
			newTimer.Callback = (IntPtr)callback;
			newTimer.TypeFlag = (byte)(isOnEntity ? 1 : 0);
			newTimer.EndTick = _wheel[0].CurrentTick + inFrames;

			int Lv = CalcLevel(inFrames);
			ScheduleTimer(ref newTimer, Lv);
			return timerId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AddTimer(delegate*<int, void> callback, int inFrames) {
			if (_freeNum <= 0) {
				ExpandCapacityByPage();
			}
			int timerId = _freePage[--_freeNum];

			ref var newTimer = ref _timer[timerId];
			newTimer.TimerId = timerId;
			newTimer.WhoAmI = -1;
			newTimer.Callback = (IntPtr)callback;
			newTimer.TypeFlag = 0;
			newTimer.EndTick = _wheel[0].CurrentTick + inFrames;

			int Lv = CalcLevel(inFrames);
			ScheduleTimer(ref newTimer, Lv);
			return timerId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveTimer(int timerId) {
			if ((uint)timerId >= (uint)_capacity)
				return false;
			ref var timer = ref _timer[timerId];
			if (timer.TimerId != timerId)
				return false;

			ref var wheel = ref _wheel[timer.Level];
			int slot = (timer.EndTick - _wheel[0].CurrentTick + wheel.CurrentTick) & _wheelMask;

			int* prevPtr = &wheel.TimerSlots[slot];
			int current = *prevPtr;

			while (current != 0) {
				if (current == timerId) {
					*prevPtr = timer.WheelSlot;
					break;
				}
				prevPtr = (int*)&_timer[current].WheelSlot;
				current = *prevPtr;
			}

			timer.TimerId = 0;
			_freePage[_freeNum++] = timerId;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ExecuteCallback(ref Timer timer) {
			if (timer.TypeFlag == 1) {
				((delegate*<int, void>)timer.Callback)(timer.WhoAmI);
			}
			else {
				((delegate*<int, void>)timer.Callback)(0);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ExecuteCallbackScalar(byte typeFlag, int whoAmI, IntPtr callbackPtr) {
			if (callbackPtr == IntPtr.Zero)
				return;
			int arg = typeFlag == 1 ? whoAmI : 0;
			((delegate*<int, void>)callbackPtr)(arg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ExpandCapacityByPage() {
			int newCapacity = _capacity * 2;
			Timer* newBufferPages = (Timer*)NativeMemory.Alloc((nuint)(newCapacity * sizeof(Timer)));
			int* newFreePages = (int*)NativeMemory.Alloc((nuint)(newCapacity * sizeof(int)));

			Unsafe.CopyBlockUnaligned(newBufferPages, _timer, (uint)(_capacity * sizeof(Timer)));
			for (int i = 0; i < _capacity; i++) {
				newFreePages[i] = _freePage[i];
			}
			for (int i = _capacity; i < newCapacity; i++) {
				newFreePages[i] = i;
			}

			NativeMemory.Free(_timer);
			NativeMemory.Free(_freePage);

			_timer = newBufferPages;
			_freePage = newFreePages;
			_freeNum += _capacity;
			_capacity = newCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NextCascade(int level) {
			ref var wheel = ref _wheel[level];
			wheel.CurrentTick = (wheel.CurrentTick + 1) & _wheelMask;
			int slot = wheel.CurrentTick;

			int* current = &wheel.TimerSlots[slot];
			int timerId = *current;
			*current = 0;

			while (timerId != 0) {
				ref var timer = ref _timer[timerId];
				int nextId = timer.WheelSlot;
				ScheduleTimer(ref timer, level - 1);
				timerId = nextId;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ScheduleTimer(ref Timer timer, int level) {
			int ticksRemain = timer.EndTick - _wheel[0].CurrentTick;
			if (ticksRemain < 0)
				ticksRemain = 0;

			ref var targetWheel = ref _wheel[level];
			int slot = (targetWheel.CurrentTick + ticksRemain) & _wheelMask;

			timer.Level = (byte)level;
			timer.WheelSlot = (short)targetWheel.TimerSlots[slot];
			targetWheel.TimerSlots[slot] = timer.TimerId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CalcLevel(int inFrames) {
			uint ticks = (uint)(inFrames - 1);
			int level = (int)(ticks >> _wheelBits);
			level = (level != 0) ? ((level - 1) >> _wheelBits) + 1 : 0;
			return Math.Min(level, _wheelLv - 1);
		}


		public static int TakeAndClearSlot(IntPtr ptr) {
			int value = *(int*)ptr;
			*(int*)ptr = 0;
			return value;
		}


		private static void UpdateILInjection(ILContext il) {
			var _IL = new ILCursor(il);

			var skipCascade1 = _IL.DefineLabel();
			var skipCascade2 = _IL.DefineLabel();
			var onSIMD = _IL.DefineLabel();
			var onScalar = _IL.DefineLabel();
			var onCascade = _IL.DefineLabel();

			var wheel0 = new VariableDefinition(il.Import(typeof(IntPtr)));
			il.Body.Variables.Add(wheel0);
			var currentSlotPtr = new VariableDefinition(il.Import(typeof(IntPtr)));
			il.Body.Variables.Add(currentSlotPtr);
			var timerBatch = new VariableDefinition(il.Import(typeof(Vector128<int>)));
			il.Body.Variables.Add(timerBatch);
			var argsBatch = new VariableDefinition(il.Import(typeof(Vector128<int>)));
			il.Body.Variables.Add(argsBatch);
			var typeFlagBatch = new VariableDefinition(il.Import(typeof(Vector128<int>)));
			il.Body.Variables.Add(typeFlagBatch);
			var maskBatch = new VariableDefinition(il.Import(typeof(Vector128<int>)));
			il.Body.Variables.Add(maskBatch);
			var currentTimerId = new VariableDefinition(il.Import(typeof(int)));
			il.Body.Variables.Add(currentTimerId);
			var nextTimerId = new VariableDefinition(il.Import(typeof(int)));
			il.Body.Variables.Add(nextTimerId);
			var tempFreeNum = new VariableDefinition(il.Import(typeof(int)));
			il.Body.Variables.Add(tempFreeNum);
			var batchTuple = new VariableDefinition(il.Import(typeof(ValueTuple<Vector128<int>, Vector128<int>, Vector128<int>>)));
			il.Body.Variables.Add(batchTuple);
			var timerStruct = new VariableDefinition(il.Import(typeof(Timer)));
			il.Body.Variables.Add(timerStruct);

			FieldInfo wheelField = typeof(MegaTimer).GetField("_wheel", BindingFlags.NonPublic | BindingFlags.Static);
			FieldInfo timerField = typeof(MegaTimer).GetField("_timer", BindingFlags.NonPublic | BindingFlags.Static);
			FieldInfo freePageField = typeof(MegaTimer).GetField("_freePage", BindingFlags.NonPublic | BindingFlags.Static);
			FieldInfo freeNumField = typeof(MegaTimer).GetField("_freeNum", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo getZeroMethod = typeof(Vector128<int>).GetProperty("Zero").GetGetMethod();
			MethodInfo opEqualityIntOnly = typeof(Vector128<int>).GetMethod("op_Equality", [typeof(Vector128<int>), typeof(Vector128<int>)]);
			MethodInfo loadTimerDataBatchMethod = typeof(MegaTimer).GetMethod("LoadTimerDataBatch", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo executeCallbackBatchMethod = typeof(MegaTimer).GetMethod("ExecuteCallbackBatch", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo batchRecycleTimersMethod = typeof(MegaTimer).GetMethod("BatchRecycleTimers", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo getNextBatchMethod = typeof(MegaTimer).GetMethod("GetNextBatch", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo getElementMethod = typeof(Vector128).GetMethod("GetElement").MakeGenericMethod(typeof(int));
			FieldInfo typeFlagField = typeof(Timer).GetField("TypeFlag");
			FieldInfo whoAmIField = typeof(Timer).GetField("WhoAmI");
			FieldInfo callbackField = typeof(Timer).GetField("Callback");
			FieldInfo wheelSlotField = typeof(Timer).GetField("WheelSlot");
			MethodInfo executeCallbackScalarMethod = typeof(MegaTimer).GetMethod("ExecuteCallbackScalar", BindingFlags.NonPublic | BindingFlags.Static);

			_IL.Emit(OpCodes.Ldsfld, wheelField);
			_IL.Emit(OpCodes.Stloc, wheel0);

			_IL.Emit(OpCodes.Ldloc, wheel0);
			_IL.Emit(OpCodes.Ldc_I4, IntPtr.Size);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Dup);
			_IL.Emit(OpCodes.Ldind_I4);
			_IL.Emit(OpCodes.Ldc_I4_1);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Ldc_I4, _wheelMask);
			_IL.Emit(OpCodes.And);
			_IL.Emit(OpCodes.Stind_I4);

			_IL.Emit(OpCodes.Ldloc, wheel0);
			_IL.Emit(OpCodes.Ldfld, typeof(Wheel).GetField("TimerSlots"));
			_IL.Emit(OpCodes.Ldloc, wheel0);
			_IL.Emit(OpCodes.Ldfld, typeof(Wheel).GetField("CurrentTick"));
			_IL.Emit(OpCodes.Conv_I4);
			_IL.Emit(OpCodes.Ldc_I4, sizeof(int));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Stloc, currentSlotPtr);


			_IL.Emit(OpCodes.Ldloc, currentSlotPtr);
			_IL.Emit(OpCodes.Call, typeof(MegaTimer).GetMethod("TakeAndClearSlot", BindingFlags.Public | BindingFlags.Static));
			_IL.Emit(OpCodes.Call, typeof(MegaTimer).GetMethod("GetNextBatch", BindingFlags.NonPublic | BindingFlags.Static));
			_IL.Emit(OpCodes.Stloc, timerBatch);
			//check point


			_IL.MarkLabel(onSIMD);
			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Call, getZeroMethod);
			_IL.Emit(OpCodes.Call, opEqualityIntOnly);
			_IL.Emit(OpCodes.Brtrue, onScalar);

			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Call, loadTimerDataBatchMethod);
			_IL.Emit(OpCodes.Stloc, batchTuple);

			//item1
			_IL.Emit(OpCodes.Ldloca, batchTuple);
			_IL.Emit(OpCodes.Ldfld, il.Module.ImportReference(typeof(ValueTuple<Vector128<int>, Vector128<int>, Vector128<int>>).GetField("Item1")));
			_IL.Emit(OpCodes.Stloc, argsBatch);

			//item2
			_IL.Emit(OpCodes.Ldloca, batchTuple);
			_IL.Emit(OpCodes.Ldfld, il.Module.ImportReference(typeof(ValueTuple<Vector128<int>, Vector128<int>, Vector128<int>>).GetField("Item2")));
			_IL.Emit(OpCodes.Stloc, typeFlagBatch);

			//item3
			_IL.Emit(OpCodes.Ldloca, batchTuple);
			_IL.Emit(OpCodes.Ldfld, il.Module.ImportReference(typeof(ValueTuple<Vector128<int>, Vector128<int>, Vector128<int>>).GetField("Item3")));
			_IL.Emit(OpCodes.Stloc, maskBatch);

			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Ldloc, argsBatch);
			_IL.Emit(OpCodes.Ldloc, typeFlagBatch);
			_IL.Emit(OpCodes.Ldloc, maskBatch);
			_IL.Emit(OpCodes.Call, executeCallbackBatchMethod);

			_IL.Emit(OpCodes.Ldsfld, freePageField);
			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Call, batchRecycleTimersMethod);


			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Ldc_I4_0);
			_IL.Emit(OpCodes.Call, getElementMethod);
			_IL.Emit(OpCodes.Call, getNextBatchMethod);
			_IL.Emit(OpCodes.Stloc, timerBatch);


			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Call, getZeroMethod);
			_IL.Emit(OpCodes.Call, opEqualityIntOnly);
			_IL.Emit(OpCodes.Brtrue, onScalar);
			_IL.Emit(OpCodes.Br, onSIMD);

			_IL.MarkLabel(onScalar);
			_IL.Emit(OpCodes.Ldloc, timerBatch);
			_IL.Emit(OpCodes.Ldc_I4_0);
			_IL.Emit(OpCodes.Call, getElementMethod);
			_IL.Emit(OpCodes.Stloc, currentTimerId);

			var loopStart = _IL.DefineLabel();
			var loopEnd = _IL.DefineLabel();
			var skipPrefetch = _IL.DefineLabel();
			//checkpoint

			_IL.MarkLabel(loopStart);
			_IL.Emit(OpCodes.Ldloc, currentTimerId);
			_IL.Emit(OpCodes.Brfalse, loopEnd);

			_IL.Emit(OpCodes.Ldsfld, timerField);
			_IL.Emit(OpCodes.Ldloc, currentTimerId);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Sizeof, typeof(Timer));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Ldobj, typeof(Timer));

			_IL.Emit(OpCodes.Ldfld, wheelSlotField);
			_IL.Emit(OpCodes.Stloc, nextTimerId);
			_IL.Emit(OpCodes.Ldloc, nextTimerId);
			_IL.Emit(OpCodes.Brfalse, skipPrefetch);

			_IL.Emit(OpCodes.Ldsfld, timerField);
			_IL.Emit(OpCodes.Ldloc, nextTimerId);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Sizeof, typeof(Timer));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Call, typeof(Sse).GetMethod("Prefetch0"));

			_IL.MarkLabel(skipPrefetch);
			_IL.Emit(OpCodes.Ldsfld, timerField);
			_IL.Emit(OpCodes.Ldloc, currentTimerId);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Sizeof, typeof(Timer));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Ldobj, typeof(Timer));
			_IL.Emit(OpCodes.Stloc, timerStruct);
			// typeFlag
			_IL.Emit(OpCodes.Ldloca, timerStruct);
			_IL.Emit(OpCodes.Ldfld, typeFlagField);
			// whoAmI
			_IL.Emit(OpCodes.Ldloca, timerStruct);
			_IL.Emit(OpCodes.Ldfld, whoAmIField);
			// callback
			_IL.Emit(OpCodes.Ldloca, timerStruct);
			_IL.Emit(OpCodes.Ldfld, callbackField);
			// µ÷ÓÃ
			_IL.Emit(OpCodes.Call, executeCallbackScalarMethod);



			_IL.Emit(OpCodes.Ldsfld, freeNumField);
			_IL.Emit(OpCodes.Dup);
			_IL.Emit(OpCodes.Stloc, tempFreeNum);
			_IL.Emit(OpCodes.Ldc_I4_1);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Stsfld, freeNumField);

			_IL.Emit(OpCodes.Ldsfld, freePageField);
			_IL.Emit(OpCodes.Ldloc, tempFreeNum);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Sizeof, typeof(int));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Ldloc, currentTimerId);
			_IL.Emit(OpCodes.Stind_I4);

			_IL.Emit(OpCodes.Ldloc, nextTimerId);
			_IL.Emit(OpCodes.Stloc, currentTimerId);
			_IL.Emit(OpCodes.Br, loopStart);

			_IL.MarkLabel(loopEnd);
			_IL.Emit(OpCodes.Br, skipCascade1);

			_IL.MarkLabel(onCascade);
			_IL.Emit(OpCodes.Ldloc, wheel0);
			_IL.Emit(OpCodes.Ldfld, typeof(Wheel).GetField("CurrentTick"));
			_IL.Emit(OpCodes.Ldc_I4, _wheelMask);
			_IL.Emit(OpCodes.Ldc_I4_0);
			_IL.Emit(OpCodes.Ceq);
			_IL.Emit(OpCodes.Brfalse, skipCascade1);

			_IL.Emit(OpCodes.Ldc_I4_1);
			_IL.Emit(OpCodes.Call, typeof(MegaTimer).GetMethod("NextCascade"));

			_IL.Emit(OpCodes.Ldsfld, wheelField);
			_IL.Emit(OpCodes.Ldc_I4_1);
			_IL.Emit(OpCodes.Conv_I);
			_IL.Emit(OpCodes.Sizeof, typeof(Wheel));
			_IL.Emit(OpCodes.Mul);
			_IL.Emit(OpCodes.Add);
			_IL.Emit(OpCodes.Ldfld, typeof(Wheel).GetField("CurrentTick"));
			_IL.Emit(OpCodes.Ldc_I4, _wheelMask);
			_IL.Emit(OpCodes.And);
			_IL.Emit(OpCodes.Brtrue, skipCascade2);

			_IL.Emit(OpCodes.Ldc_I4_2);
			_IL.Emit(OpCodes.Call, typeof(MegaTimer).GetMethod("NextCascade"));

			_IL.MarkLabel(skipCascade2);
			_IL.MarkLabel(skipCascade1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static (Vector128<int> args, Vector128<int> typeFlag, Vector128<int> mask) LoadTimerDataBatch(Vector128<int> timerIds) {
			if (Vector128.EqualsAll(timerIds, Vector128<int>.Zero)) {
				return (Vector128<int>.Zero, Vector128<int>.Zero, Vector128<int>.Zero);
			}

			int* ids = (int*)&timerIds;
			int* argsPtr = stackalloc int[4];
			int* typeFlagPtr = stackalloc int[4];
			int* maskPtr = stackalloc int[4];
			uint mask = 0;

			for (int i = 0; i < 4; i++) {
				if (ids[i] != 0) {
					mask |= (uint)(1 << i);
					ref var timer = ref _timer[ids[i]];
					argsPtr[i] = timer.WhoAmI;
					typeFlagPtr[i] = timer.TypeFlag;
					maskPtr[i] = -1;
				}
				else {
					argsPtr[i] = 0;
					typeFlagPtr[i] = 0;
					maskPtr[i] = 0;
				}
			}

			return (
				Vector128.Create(argsPtr[0], argsPtr[1], argsPtr[2], argsPtr[3]),
				Vector128.Create(typeFlagPtr[0], typeFlagPtr[1], typeFlagPtr[2], typeFlagPtr[3]),
				Vector128.Create(maskPtr[0], maskPtr[1], maskPtr[2], maskPtr[3])
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ExecuteCallbackBatch(Vector128<int> timerIds, Vector128<int> args, Vector128<int> typeFlag, Vector128<int> mask) {
			int* ids = (int*)&timerIds;
			int* argPtr = (int*)&args;
			int* typeFlagPtr = (int*)&typeFlag;
			int* maskPtr = (int*)&mask;

			for (int i = 0; i < 4; i++) {
				if (maskPtr[i] != 0 && ids[i] != 0) {
					var callback = (delegate*<int, void>)_timer[ids[i]].Callback;
					int arg = typeFlagPtr[i] == 1 ? argPtr[i] : 0;
					callback(arg);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void BatchRecycleTimers(int* freePage, Vector128<int> timerIds) {
			int* ids = (int*)&timerIds;
			int freeIndex = _freeNum;
			int count = 0;

			for (int i = 0; i < 4; i++) {
				if (ids[i] != 0) {
					freePage[freeIndex + count] = ids[i];
					_timer[ids[i]].TimerId = 0;
					count++;
				}
			}

			_freeNum += count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector128<int> GetNextBatch(int currentId) {
			if (currentId == 0)
				return Vector128<int>.Zero;

			int* ptr = stackalloc int[4] { 0, 0, 0, 0 };
			ptr[0] = currentId;
			ptr[1] = _timer[currentId].WheelSlot;

			if (ptr[1] != 0) {
				ptr[2] = _timer[ptr[1]].WheelSlot;
				if (ptr[2] != 0) {
					ptr[3] = _timer[ptr[2]].WheelSlot;
				}
			}

			return Vector128.Create(ptr[0], ptr[1], ptr[2], ptr[3]);
		}

		public override void Unload() {
			_updateTickHook?.Dispose();
			_updateTickHook = null;

			if (_wheel != null) {
				for (int i = 0; i < _wheelLv; i++) {
					if (_wheel[i].TimerSlots != null) {
						NativeMemory.Free(_wheel[i].TimerSlots);
					}
				}
				NativeMemory.Free(_wheel);
				_wheel = null;
			}

			if (_timer != null) {
				NativeMemory.Free(_timer);
				_timer = null;
			}

			if (_freePage != null) {
				NativeMemory.Free(_freePage);
				_freePage = null;
			}
		}
	}
}