using ArknightsMod.Assets.Effects;
using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.NPCs.Friendly;
using ArknightsMod.Content.Players;
using ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		public static int OriginiumIngotCurrencyId;
		internal Closure.AOSystem CurrentAO;
		/// <summary>
		/// 空材质
		/// </summary>
		/// 
		public const string noTexture = "ArknightsMod/Assets/null";

		/// <summary>
		/// 音效目录
		/// </summary>
		public const string PathSoundCommon = "ArknightsMod/Assets/Sound/";
		public const string PathProjectileExclusives = "ArknightsMod/Content/Projectiles/";
		public static Asset<Effect> IACTSW;//冲击波涟漪效果shader（如IACT）
		public static Asset<Effect> AACTTP;//缩小效果shader（AACT传送）
		public static Asset<Effect> AACTOC;//变色效果shader（AACT）
		public static Asset<Effect> AACTOC2;//反色效果shader（AACT转阶段）
		public static Asset<Effect> LightRing;//光环shader（AACT二阶段）
		public static Asset<Effect> CollapsedExplosionPart1;//坍缩爆炸效果（内核）（AACT二阶段）
		public static Asset<Effect> CollapsedExplosionPart2;//坍缩爆炸效果（描边）（AACT二阶段）
		public static Asset<Effect> AACTSTG3RBFence;//红蓝光栅效果（AACT三阶段）
		public static Asset<Effect> AACTSTG3RBNoise;//红蓝噪声效果（AACT三阶段）
		public static Asset<Effect> FNTwistedRing;//霜星限制阈（扭曲环效果）
		public static Asset<Effect> LavaExplosionShaderEffect;//炎熔的爆炸效果
		public static Asset<Effect> LupineKnifeLight;//狼之绯刀光（顶点 trail 着色器，非屏幕滤镜）
		public static Asset<Effect> ReedFlameTrail;//焰影苇草火焰拖尾（顶点 trail 着色器，非屏幕滤镜）
		public const string AssetPath = "ArknightsMod/Sound/";

		public override void Load() {
			// 热键必须在这里注册：KeybindLoader 只认 Mod.Load() 期间的注册，
			// 放在 ModSystem.Load() 里不会生效（详见 ArknightsKeybinds 的注释）。
			Content.ArknightsKeybinds.Register(this);
			UpgradeItemBase.LoadLevelData(this);
			UpgradeWeaponBase.LoadSkillData(this);
			// Registers a new custom currency
			OrundumCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OrundumCurrency(ModContent.ItemType<Orundum>(), 9999L, "Mods.ArknightsMod.Currencies.OrundumCurrency"));
			OriginiumIngotCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OriginiumIngotCurrency(ModContent.ItemType<OriginiumIngot>(), 9999L));
			//shader
			if (Main.netMode != NetmodeID.Server) {
				IACTSW = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/IACTSW", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["IACTSW"] = new Filter(new ScreenShaderData(IACTSW, "IACTSW"), EffectPriority.VeryHigh);
				Filters.Scene["IACTSW"].Load();

				AACTTP = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTTP", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["AACTTP"] = new Filter(new ScreenShaderData(AACTTP, "AACTTP"), EffectPriority.VeryHigh);
				Filters.Scene["AACTTP"].Load();

				AACTOC = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["AACTOC"] = new Filter(new ScreenShaderData(AACTOC, "AACTOC"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC"].Load();

				AACTOC2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC2", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["AACTOC2"] = new Filter(new ScreenShaderData(AACTOC2, "AACTOC2"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC2"].Load();

				LightRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/LightRing", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["LightRing"] = new Filter(new ScreenShaderData(LightRing, "LightRing"), EffectPriority.VeryHigh);
				Filters.Scene["LightRing"].Load();

				CollapsedExplosionPart1 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart1", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["CollapsedExplosionPart1"] = new Filter(new ScreenShaderData(CollapsedExplosionPart1, "CollapsedExplosionPart1"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart1"].Load();

				CollapsedExplosionPart2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart2", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["CollapsedExplosionPart2"] = new Filter(new ScreenShaderData(CollapsedExplosionPart2, "CollapsedExplosionPart2"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart2"].Load();

				AACTSTG3RBFence = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBFence", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["AACTSTG3RBFence"] = new Filter(new ScreenShaderData(AACTSTG3RBFence, "AACTSTG3RBFence"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBFence"].Load();

				AACTSTG3RBNoise = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBNoise", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["AACTSTG3RBNoise"] = new Filter(new ScreenShaderData(AACTSTG3RBNoise, "AACTSTG3RBNoise"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBNoise"].Load();

				FNTwistedRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/FNTwistedRing", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["FNTwistedRing"] = new Filter(new ScreenShaderData(FNTwistedRing, "FNTwistedRing"), EffectPriority.VeryHigh);
				Filters.Scene["FNTwistedRing"].Load();

				LavaExplosionShaderEffect = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/LavaExplosionShaderEffect", ReLogic.Content.AssetRequestMode.ImmediateLoad);
				Filters.Scene["LavaExplosionShaderEffect"] = new Filter(new ScreenShaderData(LavaExplosionShaderEffect, "LavaExplosionShaderEffect"), EffectPriority.VeryHigh);
				Filters.Scene["LavaExplosionShaderEffect"].Load();

				// 狼之绯刀光：直接作用于顶点图元，不注册为屏幕滤镜
				LupineKnifeLight = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/LupineKnifeLight", ReLogic.Content.AssetRequestMode.ImmediateLoad);

				// 焰影苇草火焰拖尾：同样直接作用于顶点图元
				ReedFlameTrail = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/ReedFlameTrail", ReLogic.Content.AssetRequestMode.ImmediateLoad);
			}
			Filters.Scene["AshStorm"] = new Filter(new ScreenShaderData("FilterAsh").UseColor(1f, 0.8f, 0.5f), EffectPriority.High);

			LoadClient();
			SkyManager.Instance["ArknightsMod:UnionInvadeSky"] = new UnionInvadeSky();

			MusicLoader.AddMusic(this, "Assets/OriginalMusic/AACTintro");
			MusicLoader.AddMusic(this, "Assets/OriginalMusic/AACTloop");
		}

		public override void Unload() {
			// 静态字段在模组卸载/重载时不会自动清空，留着会指向上一次加载的实例
			Content.ArknightsKeybinds.Unregister();
		}

		public static Texture2D UnionInvadeSkyTexture { get; private set; }

		private void LoadClient() {
			// 强制立即加载天空纹理（仿照灾厄）
			UnionInvadeSkyTexture = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Events/UnionInvadeSky",
				AssetRequestMode.ImmediateLoad
			).Value;

			// 调试验证
			if (UnionInvadeSkyTexture == null || UnionInvadeSkyTexture.IsDisposed)
				Logger.Error("天空纹理加载失败！");
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			short id = reader.ReadInt16();
			switch ((ArkMessageID)id) {
				case ArkMessageID.UpdateClosureShopWhenStartDay:
					NPCShopSystem.ReadUpdateClosureShop(reader);
					break;
				case ArkMessageID.UpdateCannotShop:
					NPCShopSystem.ReadUpdateCannotShop(reader);
					break;
				case ArkMessageID.RequestUpdateClosureShopWhenStartDay:
					NPCShopSystem.UpdateClosureShop(this);
					break;
				case ArkMessageID.RequestUpdateCannotShop:
					bool forcedUpdate = reader.ReadBoolean();
					NPCShopSystem.TryUpdateCannotShop(this, forcedUpdate);
					break;
				case ArkMessageID.SpawnReinforcements:
					if (IsValidClientRequest(whoAmI))
						Cannot.ReadSpawnReinforcements(reader, whoAmI);
					break;
				case ArkMessageID.CannotAggroAck:
					if (IsValidClientRequest(whoAmI))
						CannotAggroPlayer.ServerApplyAck(whoAmI);
					break;
				case ArkMessageID.CannotLifeTokenSync:
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						int token = reader.ReadInt32();
						ModContent.GetInstance<CannotLifeGateSystem>().ApplyNetworkLifeToken(token);
					}
					break;
				case ArkMessageID.CoffeeMachineRequest:
					if (IsValidClientRequest(whoAmI))
						WaterDispenserTile.TryGiveCoffee(Main.player[whoAmI]);
					break;
				case ArkMessageID.ElevatorRequestFloor:
					if (IsValidClientRequest(whoAmI)) {
						int teId = reader.ReadInt32();
						int floorBottomY = reader.ReadInt32();
						global::ArknightsMod.Content.Tiles.TEElevator.ApplyMoveRequest(teId, floorBottomY, whoAmI);
					}
					break;
				case ArkMessageID.AkStructureRequestDeploy:
					if (IsValidClientRequest(whoAmI))
						global::ArknightsMod.Systems.Structures.AkStructureDeploySystem.ReceiveDeployRequest(reader, whoAmI);
					break;
				case ArkMessageID.AkStructurePlacedEffect:
					// 放置特效是服务器广播结果，专服绝不能执行客户端绘制路径。
					if (Main.netMode == NetmodeID.MultiplayerClient)
						global::ArknightsMod.Systems.Structures.AkStructureDeploySystem.ReceivePlacedEffect(reader);
					break;
				case ArkMessageID.DeploymentCostAbsorbRequest:
					if (IsValidClientRequest(whoAmI))
						DeploymentCost.ReceiveAbsorbRequest(reader, whoAmI);
					break;
				case ArkMessageID.DeploymentCostAbsorbGrant:
					if (Main.netMode == NetmodeID.MultiplayerClient)
						DeploymentCost.ReceiveAbsorbGrant(reader);
					break;
				case ArkMessageID.DeploymentCostAbsorbResult:
					if (IsValidClientRequest(whoAmI))
						DeploymentCost.ReceiveAbsorbResult(reader, whoAmI);
					break;
			}
		}

		private static bool IsValidClientRequest(int whoAmI) {
			return Main.netMode == NetmodeID.Server &&
				(uint)whoAmI < Main.maxPlayers &&
				Main.player[whoAmI].active;
		}

		public enum ArkMessageID : short {
			UpdateClosureShopWhenStartDay,
			RequestUpdateClosureShopWhenStartDay,
			UpdateCannotShop,
			RequestUpdateCannotShop,
			SpawnReinforcements,
			CannotAggroAck,
			CannotLifeTokenSync,
			CoffeeMachineRequest,
			ElevatorRequestFloor,
			// 曾经的 PortableSafehouseRequestDeploy 在这个位置，随"便携自建安全屋"一起移除。
			// 枚举值是按顺序自动分配的，删掉它会让后面两项的数值整体前移一位——这不影响联机，
			// 因为 tModLoader 本来就要求服务器和客户端的模组版本完全一致才能进服，
			// 不存在新旧版本互相收发包的情况。
			AkStructureRequestDeploy,
			AkStructurePlacedEffect,
			DeploymentCostAbsorbRequest,
			DeploymentCostAbsorbGrant,
			DeploymentCostAbsorbResult,
		}
	}
	//public class Ex : GlobalNPC
	//{
	//public override void SetDefaults(NPC entity) {
	//if (entity.ModNPC is not null && entity.ModNPC.Mod == Mod) {
	//entity.lifeMax = (int)(entity.lifeMax * 1.2f);
	//entity.life = entity.lifeMax;
	//entity.damage = (int)(entity.damage * 1.2f);
	//}
	//}
	//}
	
	
	


}
