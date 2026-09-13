using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Common;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using ArknightsMod.Content.Items.Material.ReclamAlgor;

namespace ArknightsMod.Content.NPCs.Enemy.ReclamationAlgorithm.Cragpincer
{
	/// <summary>
	/// 峭壁钳
	/// </summary>
	public class Cragpincer : ModNPC
	{
		private const string _tex = "ArknightsMod/Content/NPCs/Enemy/ReclamationAlgorithm/Cragpincer/Cragpincer";
		public override string Texture => _tex + "_Common";
		private int _danceCount = 0;
		private int _time = 0;
		private int _time2 = 0;
		private int _fra = 0;
		private Player _target = null;
		private bool _hitting = false;
		private int _hateTime = 0;
		private string _danceName = "Common";
		private enum State {
			Common,
			Enemy,
			Hit,
			/// <summary>
			/// 跳↓舞↑
			/// </summary>
			Dance
		}
		private State _state;
		public override void SetDefaults()
		{
			_state = State.Common;
			NPC.width = 46;
			NPC.height = 30;
			NPC.damage = 40;
			NPC.lifeMax = 150;
			NPC.defense = 80;
			NPC.friendly = true;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = Item.buyPrice(0, 0, 15, 0);
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.aiStyle = -1;
		}
		public override void OnSpawn(IEntitySource source)
		{
			_state = State.Common;
			NPC.friendly = true;
		}
		private bool HitByPlayer()
		{
			Rectangle rectangle = new((int)NPC.Center.X, (int)NPC.Center.Y, 46, 34);
			foreach(Projectile proj in Main.projectile)
			{
				if(proj != null && proj.active && proj.friendly && proj.Hitbox.Intersects(rectangle) && proj.damage > 0)
				{
					return true;
				}
			}
			return false;
		}
		public override void AI()
		{

			switch (_state)
			{
				case State.Common:
				{
					if(HitByPlayer())
					{
						_state = State.Enemy;
						NPC.friendly = false;
					}
					else
					{
						if(_time2 > 60 * 5)
						{
							_state = State.Dance;
							_time2 = 0;
							NPC.velocity = Vector2.Zero;
							if(Main.rand.NextBool(3,5))
							{
								NPC.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), 0f);
								if(Math.Abs(NPC.velocity.X) < 1.5f)
								{
									NPC.velocity.X = 1.5f * (Main.rand.NextBool(1, 2) ? -1f : 1f);
								}
							}
							else
							{
								NPC.velocity = Vector2.Zero;
							}
						}
						else
						{
							_time2++;
						}
					}
					break;
				}
				case State.Enemy:
				{
					if(TryGetPlayer(out var target))
					{
						_target = target;
						if(Vector2.DistanceSquared(NPC.Center,_target.Center) < 120)
						{
							NPC.velocity = Vector2.Zero;
							_state = State.Hit;
						}
						else
						{
							NPC.velocity = (_target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 2f;

							Tilemap map = Main.tile;
							if(!map[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16) - 1].HasTile)
							{
								NPC.velocity.Y = 0.5f;
							}
						}
					}
					break;
				}
				case State.Hit:
					{
						if(_target == null)
						{
							_state = State.Common;
						}
						if(!_hitting)
						{
							if(Vector2.DistanceSquared(NPC.Center, _target.Center) > 20)
							{
								_state = State.Enemy;
							}
						}
						break;
					}
				case State.Dance:
					{
						switch (_danceName)
						{
							case "Common":
								{
									if (NPC.ai[2] < 5)
									{
										_fra = 0;
										NPC.ai[2]++;
									}
									else
									{
										_fra = 0;
										NPC.ai[2] = 0;
										_danceName = "Walk";
									}
									break;
								}
							case "Hit":
								{
									if (NPC.ai[2] < 5)
									{
										NPC.ai[2]++;
									}
									else
									{
										if (_fra == 4)
										{
											if (_danceCount > 3)
											{
												_fra = 0;
												NPC.ai[2] = 0;
												_danceCount = 0;
												_danceName = "Common";
												_state = State.Common;
												return;
											}
											else
											{
												_danceCount++;
												_fra = 0;
												NPC.ai[2] =0;
												_danceName = "Common";
											}
										}
										else
										{
											_fra++;
											NPC.ai[2] = 0;
										}
									}
									break;
								}
							case "Walk":
								{
									if (NPC.ai[2] < 5)
									{
										NPC.ai[2]++;
									}
									else
									{
										if (_fra == 3)
										{
											_fra = 0;
											NPC.ai[2] = 0;
											_danceName = "Hit";
											return;
										}
										else
										{
											_fra++;
											NPC.ai[2] = 0;
										}
									}
									break;
								}
						}
						NPC.velocity = Vector2.Zero;
						break;
					}
			}

			if (_state == State.Enemy || _state == State.Hit) {
				if (_hateTime > 900) {
					_state = State.Common;
					_hateTime = 0;
					NPC.friendly = true;
				}
				_hateTime++;
			}
			else {
				_hateTime = 0;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if(_state == State.Common)
			{
				hurtInfo.Damage = 0;
			}
			else
			{
				base.OnHitPlayer(target,hurtInfo);
			}
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			_fra = 0;
			_state = (_state == State.Hit ? State.Hit : State.Enemy);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			_fra = 0;
			_state = (_state == State.Hit ? State.Hit : State.Enemy);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if(spawnInfo.Player.ZoneBeach)
			{
				return 0.5f;
			}
			else
			{
				return 0f;
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrabClaw>(), 1, 2, 2));
		}
		private bool TryGetPlayer(out Player target)
		{
			Player _best = null;
			float _bestDis = float.MaxValue;
			foreach(Player player in Main.player)
			{
				if(player.active && !player.dead)
				{
					float now = Vector2.DistanceSquared(player.Center, NPC.Center);
					if (now < _bestDis)
					{
						_bestDis = now;
						_best = player;
					}
				}
			}
			target = _best;
			return _best != null;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			switch (_state)
			{
				case State.Enemy:
				{
						if (_time > 2)
						{
							_time = 0;
							_fra++;
						}
						else {
							_time++;
						}
						if (_fra > 3)
						{
							_fra = 0;
						}
						float rot = 0;
						if (_target != null)
						{
							rot = (_target.Center - NPC.Center).ToRotation();
						}
						Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Walk_" + _fra).Value;
						spriteBatch.Draw(texture,NPC.Center - Main.screenPosition, null, Color.White, rot > -MathHelper.PiOver2 ? 0f : MathHelper.Pi, texture.GetTextureSize() / 2f, 1f, rot > (-MathHelper.PiOver2) ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
						break;
				}
				case State.Hit:
				{
						if (_time > 2)
						{
							_time = 0;
							_fra++;
						}
						else
						{
							_time++;
						}
						if (_fra > 4)
						{
							_fra = 0;
							if(_target == null)
							{
								_state = State.Common;
							}
							else if(Vector2.DistanceSquared(NPC.Center, _target.Center) > 120)
							{
								_hitting = false;
							}
							else
							{
								_hitting = true;
							}
						}
						float rot = 0;
						if(_target != null)
						{
							rot = (_target.Center - NPC.Center).ToRotation();
						}
						Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Hit_" + _fra).Value;
						spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, rot > -MathHelper.PiOver2 ? 0f : MathHelper.Pi, texture.GetTextureSize() / 2f, 1f,rot > (-MathHelper.PiOver2) ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
						break;
				}
				case State.Common:
				{
						if (NPC.velocity != Vector2.Zero) {

							if (_time > 2) {
								_time = 0;
								_fra++;
							}
							else {
								_time++;
							}
							if (_fra > 3) {
								_fra = 0;
							}
							float rot;
							if (_target != null) {
								rot = (_target.Center - NPC.Center).ToRotation();
							}
							else
							{
								rot = NPC.velocity.ToRotation();
							}
							Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Walk_" + _fra).Value;
							spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, rot > -MathHelper.PiOver2 ? 0f : MathHelper.Pi, texture.GetTextureSize() / 2f, 1f, NPC.velocity.X < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
							
						}
						else
						{
							Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Common").Value;
							
							spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, 0f, texture.GetTextureSize() / 2f, 1f, SpriteEffects.None, 0f);
						}

						break;
				}
				case State.Dance:
					{
						switch (_danceName)
						{
							case "Common":
								{
									Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Common").Value;
									spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, 0f, texture.GetTextureSize() / 2f, 1f, SpriteEffects.None, 0f);
									break;
								}
							case "Hit":
								{
									Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Hit_" + _fra).Value;
									spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, 0f, texture.GetTextureSize() / 2f, 1f, SpriteEffects.None, 0f);
									break;
								}
							case "Walk":
								{
									Texture2D texture = ModContent.Request<Texture2D>(_tex + "_Walk_" + _fra).Value;
									spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, Color.White, 0f, texture.GetTextureSize() / 2f, 1f, SpriteEffects.None, 0f);
									break;
								}
						}
						break;
					}
            }
			return false;
		}
	}
}
