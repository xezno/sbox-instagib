﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instagib.Entities;
using Instagib.GameTypes;
using Instagib.UI;
using Sandbox;

namespace Instagib
{
	public partial class Game : Sandbox.Game
	{
		private static InstagibHud hud;
		public static Game Instance;

		[Net] public BaseGameType GameType { get; set; }

		public Game()
		{
			Precache.Add( "particles/gib_blood.vpcf" );
			Precache.Add( "particles/speed_lines.vpcf" );
			Precache.Add( "sounds/jump.vsnd" );

			Precache.Add( "weapons/railgun/particles/railgun_beam.vpcf" );
			Precache.Add( "weapons/railgun/particles/railgun_pulse.vpcf" );
			Precache.Add( "weapons/railgun/sounds/railgun_fire.vsnd" );

			if ( IsClient )
			{
				PlayerSettings.Load();
			}

			if ( IsServer )
			{
				hud = new InstagibHud();

				if ( Global.MapName.Contains( "ctf" ) ) // TODO: Revisit this, not a great solution
				{
					GameType = new CtfGameType();
				}
				else
				{
					GameType = new FfaGameType();
				}
			}

			Instance = this;
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			List<Entity> spawnPointEntities = new();

			Entity.All.OfType<SpawnPoint>().ToList().ForEach( x => spawnPointEntities.Add( x ) );
			Entity.All.OfType<InstagibPlayerSpawn>().ToList().ForEach( x => spawnPointEntities.Add( x ) );

			var spawnpoint = spawnPointEntities
									.OrderBy( x => Guid.NewGuid() )     // order them by random
									.FirstOrDefault();                  // take the first one

			if ( spawnpoint == null )
			{
				Log.Error( $"Couldn't find spawnpoint for {pawn}!" );
				return;
			}

			pawn.Transform = spawnpoint.Transform;
			base.MoveToSpawnpoint( pawn );
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );
			CurrentState.OnPlayerJoin( cl );

			var player = new Player( cl );
			cl.Pawn = player;
			GameType.AssignPlayerTeam( player );
			player.Respawn();
		}

		public override void DoPlayerNoclip( Client cl )
		{
			if ( cl.PlayerId != InstagibGlobal.AlexSteamId )
				return;

			base.DoPlayerNoclip( cl );
		}

		public override void DoPlayerDevCam( Client cl )
		{
			if ( cl.PlayerId != InstagibGlobal.AlexSteamId )
				return;

			base.DoPlayerDevCam( cl );
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );
			CurrentState.OnPlayerLeave( cl );
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was killed" );

			if ( pawn is not Player victim )
				return;

			CurrentState.OnDeath( victim.Client );

			// HACK: Assign a respawn timer for this player
			async Task RespawnTimer()
			{
				await Task.DelaySeconds( 3.0f );
				PlayerRespawnRpc( To.Single( victim ) );
			}
			_ = RespawnTimer();

			//
			// Get attacker info
			//
			if ( pawn.LastAttacker is not Player attacker )
			{
				PlayerDiedRpc( To.Single( victim ), null );
				OnKilledMessage( 0, "", (long)client.PlayerId, client.Name, "died" );
				return;
			}

			// Killstreak tracking
			attacker.CurrentStreak++;
			CurrentState.OnKill( attacker.Client, victim.Client );

			PlayerDiedRpc( To.Single( victim ), attacker );

			//
			// Give out medals to the attacker
			//
			List<Medal> medals = Medals.GetMedalsForKill( attacker, victim );

			string[] medalArr = new string[medals.Count];
			for ( int i = 0; i < medals.Count; ++i )
				medalArr[i] = medals[i].Name;

			// Display "YOU FRAGGED" message
			PlayerKilledRpc( To.Single( attacker ), attacker, victim, medalArr );

			var attackerClient = attacker.Client;
			OnKilledMessage( (long)attackerClient.PlayerId, attackerClient.Name, (long)client.PlayerId, client.Name, "Railgun" );
		}

		/// <summary>
		/// Called clientside from OnKilled on the server to add kill messages to the killfeed. 
		/// </summary>
		[ClientRpc]
		public virtual void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
		{
			KillFeed.Current.AddEntry( leftid, left, rightid, right, method );
		}

		[ClientRpc]
		public void PlayerRespawnRpc()
		{
			InstagibHud.currentHud?.OnRespawn();
		}

		[ConCmd.Server( "recreatehud", Help = "Recreate hud object" )]
		public static void RecreateHud()
		{
			hud.Delete();
			hud = new();
			Log.Info( "Recreated HUD" );
		}

		[ClientRpc]
		public void PlayerDiedRpc( Player attacker )
		{
			// Attacker, victim
			InstagibHud.currentHud.OnDeath( attacker?.Client?.Name ?? "Yourself" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( Player attacker, Player victim, string[] medals )
		{
			// Attacker, victim
			InstagibHud.currentHud.OnKilledMessage( attacker, victim, medals );
		}
	}
}
