using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Friendly
{
	// 「部署费用」：类似 Minecraft 经验球的可吸收资源球——纯代码绘制、无贴图。
	//   · 作为 NPC 存在，但无法被任何玩家/生物/NPC 的攻击命中或击败（immortal）。
	//   · 在场约 10 秒仍未被吸收则自行淡出消失。
	//   · 1 点：很小的绿色方形像素点 + 绿色外发光，自身发光强度「呼吸」般忽明忽暗。
	//   · 附近有 5 个 1 点时，自动合成为单个 5 点（体积更大、颜色偏白绿）。
	//   · 玩家手持本模组（含技力系统）的武器、且未处于技能开启状态（技力自然回复中）时，
	//     靠近即被吸收，直接把点数加到技力条上（不进背包）；1 点回 1 技力，5 点回 5 技力。
	//   · 靠近玩家一定距离会被主动吸引；若当前玩家状态无法吸收，则改为环绕玩家旋转。
	// ai[0] = 点数（1 或 5）。
	public class DeploymentCost : ModNPC
	{
		private const int LifeTicks   = 600; // 存活约 10 秒（60TPS）
		private const int FadeInTicks = 8;
		private const int FadeOutTicks = 45;

		private const float PullRange   = 190f; // 进入此范围开始被玩家吸引 / 环绕
		private const float AbsorbRange = 20f;  // 进入此范围（且可吸收）即被吸收
		private const float OrbitRadius = 46f;   // 无法吸收时环绕玩家的半径
		private const float MergeRange  = 70f;   // 5 个 1 点在此范围内合成 5 点

		private static readonly Color OneCore   = new(70, 235, 70);   // 1 点核心：纯绿
		private static readonly Color OneGlow   = new(40, 220, 40);   // 1 点外发光
		private static readonly Color FiveCore  = new(180, 255, 185); // 5 点核心：偏白的绿
		private static readonly Color FiveGlow  = new(120, 245, 140); // 5 点外发光

		private static BasicEffect _basic;

		private int _age;
		private float _breath;      // 呼吸相位
		private bool _fadingOut;
		private int _fadeAge;
		private float _orbitAngle;
		private bool _orbitInit;
		private int _absorbRequestCooldown;
		private int _pendingRequestToken;
		private int _reservedPlayer = -1;
		private int _reservationTicks;
		private int _reservationToken;
		private static int _nextClientRequestToken;

		public int Value {
			get => Math.Max(1, (int)NPC.ai[0]);
			set => NPC.ai[0] = value;
		}
		private bool IsFive => Value >= 5;

		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.ActsLikeTownNPC[Type] = false;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
		}

		public override void Unload() {
			Main.QueueMainThreadAction(() => {
				_basic?.Dispose();
				_basic = null;
			});
		}

		public override void SetDefaults() {
			NPC.width = NPC.height = 10;
			NPC.aiStyle = -1;
			NPC.friendly = true;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 1;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.chaseable = false;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
			NPC.dontCountMe = true;
			NPC.npcSlots = 0f;
			NPC.HitSound = null;
			NPC.DeathSound = null;
		}

		// 永不被任何来源命中
		public override bool? CanBeHitByItem(Player player, Item item) => false;
		public override bool? CanBeHitByProjectile(Projectile projectile) => false;
		public override bool CanBeHitByNPC(NPC attacker) => false;
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
		public override bool CheckActive() => false; // 生命周期自行管理，不因玩家远离而被回收

		/// <summary>生成指定点数的单个部署费用球（1 或 5），并给一个像经验球一样弹出的初速度。</summary>
		public static void SpawnOrb(IEntitySource source, Vector2 position, int points) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			points = points >= 5 ? 5 : 1;
			int idx = NPC.NewNPC(source, (int)position.X, (int)position.Y, ModContent.NPCType<DeploymentCost>(), ai0: points);
			if (idx >= 0 && idx < Main.maxNPCs) {
				NPC npc = Main.npc[idx];
				npc.velocity = new Vector2(Main.rand.NextFloat(-2.4f, 2.4f), Main.rand.NextFloat(-3.6f, -1.2f));
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.SyncNPC, number: idx);
			}
		}

		/// <summary>把任意总点数拆成若干 5 点 + 1 点球生成（便于技能一次性给予一批部署费用）。</summary>
		public static void SpawnAmount(IEntitySource source, Vector2 position, int totalPoints) {
			if (Main.netMode == NetmodeID.MultiplayerClient || totalPoints <= 0)
				return;
			int fives = totalPoints / 5;
			int ones = totalPoints % 5;
			for (int i = 0; i < fives; i++)
				SpawnOrb(source, position, 5);
			for (int i = 0; i < ones; i++)
				SpawnOrb(source, position, 1);
		}

		public static void ReceiveAbsorbRequest(BinaryReader reader, int sender) {
			int npcIndex = reader.ReadInt16();
			int requestToken = reader.ReadInt32();
			int itemType = reader.ReadInt32();
			int skillIndex = reader.ReadByte();
			if (Main.netMode != NetmodeID.Server || (uint)sender >= Main.maxPlayers || (uint)npcIndex >= Main.maxNPCs)
				return;

			Player player = Main.player[sender];
			NPC npc = Main.npc[npcIndex];
			if (!player.active || player.dead || player.HeldItem.type != itemType
				|| player.HeldItem.ModItem is not UpgradeWeaponBase || (uint)skillIndex >= 3
				|| !npc.active || npc.ModNPC is not DeploymentCost cost
				|| Vector2.DistanceSquared(player.Center, npc.Center) > (AbsorbRange + 36f) * (AbsorbRange + 36f))
				return;
			if (requestToken == 0 || cost._reservationTicks > 0)
				return;

			int points = cost.Value;
			Vector2 position = npc.Center;
			cost._reservedPlayer = sender;
			cost._reservationTicks = 6 * 60;
			cost._reservationToken = requestToken;

			ModPacket packet = ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().GetPacket();
			packet.Write((short)global::ArknightsMod.ArknightsMod.ArkMessageID.DeploymentCostAbsorbGrant);
			packet.Write((short)npcIndex);
			packet.Write(requestToken);
			packet.Write(itemType);
			packet.Write((byte)skillIndex);
			packet.Write((byte)points);
			packet.Write(position.X);
			packet.Write(position.Y);
			packet.Send(sender);
		}

		public static void ReceiveAbsorbGrant(BinaryReader reader) {
			int npcIndex = reader.ReadInt16();
			int requestToken = reader.ReadInt32();
			int itemType = reader.ReadInt32();
			int skillIndex = reader.ReadByte();
			int points = reader.ReadByte();
			Vector2 position = new(reader.ReadSingle(), reader.ReadSingle());
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			DeploymentCost cost = null;
			if ((uint)npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
				cost = Main.npc[npcIndex].ModNPC as DeploymentCost;
			bool tokenMatches = cost != null && cost._pendingRequestToken == requestToken;
			Player player = Main.LocalPlayer;
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			bool accepted = tokenMatches && player.HeldItem.type == itemType && weaponPlayer.Skill == skillIndex
				&& weaponPlayer.AbsorbDeploymentCost(points);
			if (tokenMatches) {
				cost._pendingRequestToken = 0;
				// 接受后给服务器 Result/SyncNPC 留出往返时间，避免同一球在销毁同步前再次请求。
				cost._absorbRequestCooldown = accepted ? 60 : 0;
			}
			if (accepted)
				SpawnAbsorbEffect(position, points);

			ModPacket response = ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().GetPacket();
			response.Write((short)global::ArknightsMod.ArknightsMod.ArkMessageID.DeploymentCostAbsorbResult);
			response.Write((short)npcIndex);
			response.Write(requestToken);
			response.Write(accepted);
			response.Send();
		}

		public static void ReceiveAbsorbResult(BinaryReader reader, int sender) {
			int npcIndex = reader.ReadInt16();
			int requestToken = reader.ReadInt32();
			bool accepted = reader.ReadBoolean();
			if (Main.netMode != NetmodeID.Server || (uint)npcIndex >= Main.maxNPCs)
				return;

			NPC npc = Main.npc[npcIndex];
			if (!npc.active || npc.ModNPC is not DeploymentCost cost
				|| cost._reservationTicks <= 0 || cost._reservedPlayer != sender
				|| cost._reservationToken != requestToken)
				return;

			cost._reservedPlayer = -1;
			cost._reservationTicks = 0;
			cost._reservationToken = 0;
			if (!accepted)
				return;

			npc.active = false;
			NetMessage.SendData(MessageID.SyncNPC, number: npcIndex);
		}

		public override void AI() {
			_age++;
			_breath += 0.09f;
			if (_absorbRequestCooldown > 0 && --_absorbRequestCooldown <= 0)
				_pendingRequestToken = 0;
			if (Main.netMode == NetmodeID.Server && _reservationTicks > 0 && --_reservationTicks <= 0) {
				_reservedPlayer = -1;
				_reservationToken = 0;
			}

			// 轻微阻尼 + 缓慢上浮的悬停感（未进入吸引状态时）
			NPC.velocity *= 0.94f;

			Player player = FindNearestEligiblePlayer(out float dist, out WeaponPlayer mp, out bool canAbsorb);
			if (player != null && dist < PullRange) {
				if (canAbsorb) {
					_orbitInit = false;
					if (dist < AbsorbRange && Main.netMode != NetmodeID.Server) {
						AbsorbBy(player, mp);
						return;
					}
					// 越近吸得越快
					float t = 1f - MathHelper.Clamp(dist / PullRange, 0f, 1f);
					Vector2 dir = Vector2.Normalize(player.Center - NPC.Center);
					float speed = MathHelper.Lerp(2.4f, 8.5f, t);
					NPC.velocity = Vector2.Lerp(NPC.velocity, dir * speed, 0.25f);
				}
				else {
					OrbitAround(player);
				}
			}

			bool transactionPending = _reservationTicks > 0 || _pendingRequestToken != 0;
			if (!transactionPending)
				TryMerge();

			// 生命周期：到时开始淡出，淡出完成后移除
			if (transactionPending) {
				// 两阶段吸收尚未提交时冻结过期/合并，保证客户端确认前 NPC 不会换值或消失。
				_fadingOut = false;
				_fadeAge = 0;
				_age = Math.Min(_age, LifeTicks - 1);
			}
			else {
				if (!_fadingOut && _age >= LifeTicks)
					_fadingOut = true;
				if (_fadingOut) {
					_fadeAge++;
					if (_fadeAge > FadeOutTicks)
						Despawn();
				}
			}

			Vector3 lightCol = IsFive ? new Vector3(0.4f, 0.85f, 0.45f) : new Vector3(0.18f, 0.55f, 0.2f);
			Lighting.AddLight(NPC.Center, lightCol.X, lightCol.Y, lightCol.Z);
		}

		private Player FindNearestEligiblePlayer(out float dist, out WeaponPlayer mp, out bool canAbsorb) {
			dist = float.MaxValue;
			mp = null;
			canAbsorb = false;
			Player best = null;
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player p = Main.player[i];
				if (!p.active || p.dead)
					continue;
				float d = Vector2.Distance(p.Center, NPC.Center);
				if (d < dist) {
					dist = d;
					best = p;
				}
			}
			if (best != null) {
				mp = best.GetModPlayer<WeaponPlayer>();
				// 服务器没有同步每个客户端的技能槽/技力细节，只负责把球拉到持有技能武器的玩家附近；
				// 最终资格由拥有者客户端请求时的本地状态决定，再由服务器确认距离并销毁球。
				canAbsorb = Main.netMode == NetmodeID.Server
					? best.HeldItem.ModItem is UpgradeWeaponBase
					: mp.CanAbsorbDeploymentCost();
			}
			return best;
		}

		private void OrbitAround(Player player) {
			Vector2 toOrb = NPC.Center - player.Center;
			if (!_orbitInit) {
				_orbitAngle = toOrb.ToRotation();
				_orbitInit = true;
			}
			_orbitAngle += 0.085f;
			float curR = toOrb.Length();
			float r = MathHelper.Lerp(curR, OrbitRadius, 0.12f);
			Vector2 target = player.Center + _orbitAngle.ToRotationVector2() * r;
			NPC.velocity = (target - NPC.Center) * 0.5f;
		}

		private void AbsorbBy(Player player, WeaponPlayer mp) {
			if (Main.netMode == NetmodeID.SinglePlayer) {
				if (mp.AbsorbDeploymentCost(Value))
					SpawnAbsorbEffect(NPC.Center, Value);
				Despawn();
				return;
			}

			if (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer
				|| _absorbRequestCooldown > 0)
				return;

			_absorbRequestCooldown = 4 * 60;
			_pendingRequestToken = unchecked(++_nextClientRequestToken);
			if (_pendingRequestToken == 0)
				_pendingRequestToken = unchecked(++_nextClientRequestToken);
			ModPacket packet = ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().GetPacket();
			packet.Write((short)global::ArknightsMod.ArknightsMod.ArkMessageID.DeploymentCostAbsorbRequest);
			packet.Write((short)NPC.whoAmI);
			packet.Write(_pendingRequestToken);
			packet.Write(player.HeldItem.type);
			packet.Write((byte)mp.Skill);
			packet.Send();
		}

		private static void SpawnAbsorbEffect(Vector2 position, int points) {
			if (Main.dedServ)
				return;
			bool isFive = points >= 5;
			int count = isFive ? 10 : 5;
			Color col = isFive ? FiveCore : OneCore;
			for (int i = 0; i < count; i++) {
				Vector2 vel = Main.rand.NextVector2Circular(2.4f, 2.4f);
				Dust d = Dust.NewDustPerfect(position, DustID.GreenTorch, vel, 0, col, Main.rand.NextFloat(0.9f, 1.5f));
				d.noGravity = true;
			}
		}

		// 附近有 5 个 1 点则自动合成为单个 5 点（服务器/单机权威，最低 whoAmI 执行以避免重复）。
		private void TryMerge() {
			if (Value != 1 || _fadingOut || _age < 6 || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			List<int> ones = new();
			int myType = Type;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC other = Main.npc[i];
				if (!other.active || other.type != myType)
					continue;
				if (other.ModNPC is DeploymentCost dc && dc.Value == 1 && !dc._fadingOut
					&& dc._reservationTicks <= 0
				    && Vector2.Distance(other.Center, NPC.Center) <= MergeRange) {
					ones.Add(i);
				}
			}

			if (ones.Count < 5)
				return;

			ones.Sort();
			if (ones[0] != NPC.whoAmI)
				return; // 只由组内最低编号者执行合成

			// 自己升级为 5 点，吞掉另外 4 个 1 点
			for (int k = 1; k < 5; k++) {
				NPC eaten = Main.npc[ones[k]];
				if (eaten.ModNPC is DeploymentCost)
					eaten.active = false;
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.SyncNPC, number: ones[k]);
			}
			Value = 5;
			_age = 0;
			_fadeAge = 0;
			NPC.netUpdate = true;
			if (!Main.dedServ) {
				for (int i = 0; i < 12; i++) {
					Dust d = Dust.NewDustPerfect(NPC.Center, DustID.GreenTorch,
						Main.rand.NextVector2Circular(2.2f, 2.2f), 0, FiveCore, Main.rand.NextFloat(0.9f, 1.4f));
					d.noGravity = true;
				}
			}
		}

		private void Despawn() {
			NPC.active = false;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (Main.dedServ)
				return false;

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			float fadeIn  = MathHelper.Clamp(_age / (float)FadeInTicks, 0f, 1f);
			float fadeOut = _fadingOut ? MathHelper.Clamp(1f - _fadeAge / (float)FadeOutTicks, 0f, 1f) : 1f;
			float alpha = fadeIn * fadeOut;
			if (alpha <= 0.01f)
				return false;

			// 呼吸：发光强度在约 0.55~1.0 之间忽明忽暗
			float breath = 0.55f + 0.45f * (0.5f + 0.5f * (float)Math.Sin(_breath));

			bool five = IsFive;
			Color coreCol = (five ? FiveCore : OneCore) * alpha;
			Color glowBase = five ? FiveGlow : OneGlow;
			float coreHalf = five ? 2.6f : 1.6f;
			float glowR    = five ? 13f : 8f;

			Vector2 pos = NPC.Center - Main.screenPosition;
			var verts = new List<VertexPositionColor>(64);

			// 外发光：柔和径向渐变（中心浓、边缘透明），透明度受呼吸调制
			{
				const int seg = 14;
				Color gi = glowBase * (alpha * 0.7f * breath);
				Color go = gi; go.A = 0;
				for (int i = 0; i < seg; i++) {
					float a0 = MathHelper.TwoPi * i / seg;
					float a1 = MathHelper.TwoPi * (i + 1) / seg;
					AddTri(verts, pos,
						pos + a0.ToRotationVector2() * glowR,
						pos + a1.ToRotationVector2() * glowR, gi, go, go);
				}
			}

			// 核心：正方形「像素点」（不是圆点）
			{
				Vector2 tl = pos + new Vector2(-coreHalf, -coreHalf);
				Vector2 tr = pos + new Vector2( coreHalf, -coreHalf);
				Vector2 br = pos + new Vector2( coreHalf,  coreHalf);
				Vector2 bl = pos + new Vector2(-coreHalf,  coreHalf);
				AddTri(verts, tl, tr, br, coreCol, coreCol, coreCol);
				AddTri(verts, tl, br, bl, coreCol, coreCol, coreCol);
			}

			GraphicsDevice device = Main.instance.GraphicsDevice;
			spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			_basic.Projection = Main.GameViewMatrix.TransformationMatrix * projection;

			var arr = verts.ToArray();
			foreach (EffectPass pass in _basic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
