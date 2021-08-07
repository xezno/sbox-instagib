using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instagib.UI;
using Instagib.Utils;
using Sandbox;
using Sandbox.ScreenShake;
using Event = Sandbox.Event;

namespace Instagib
{
	[Library( "instagib", Title = "Instagib")]
	public partial class Game : Sandbox.Game
	{
		private static InstagibHud hud;
		public Stats Stats { get; set; }
		public Game()
		{
			Precache.Add( "particles/gib_blood.vpcf" );
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
			}
		}
		
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );
			
			Event.Run( "playerJoined" );

			var player = new Player();
			client.Pawn = player;
			client.SetScore( "steamid", client.SteamId );
			
			player.Respawn();

			// StartStatsRpc( To.Single( client ) );
		}

		[ClientCmd( "reconnect_stats" )]
		public static void ReconnectStatsCmd()
		{
			// (Sandbox.Game.Current as Game).StartStatsRpc( To.Single( Local.Pawn ) ); 
		}
		
		public override void DoPlayerNoclip( Client player )
		{
			if ( player.SteamId != 76561198128972602 )
				return;

			base.DoPlayerNoclip( player );
		}

		public override void DoPlayerDevCam( Client player )
		{
			if ( player.SteamId != 76561198128972602 )
				return;
			
			base.DoPlayerDevCam( player );
		}

		[ClientRpc]
		private void StartStatsRpc()
		{
			Stats = new Stats( IsServer );
			Event.Register( Stats );
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );
			Event.Run( "playerLeft" );
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was killed" );
			if ( pawn is not Player victim )
				return;
			
			// HACK: Assign a respawn timer for this player
			async Task RespawnTimer()
			{
				Log.Info( "Waiting" );
				await Task.DelaySeconds( 3.0f );
				Log.Info( "Waited" );
				PlayerRespawnRpc( To.Single( victim ) );
			}
			RespawnTimer();

			// Apply a death
			var victimClient = victim.GetClientOwner();
			victimClient.SetScore( "deaths", victimClient.GetScore<int>( "deaths" ) + 1 );

			if ( pawn.LastAttacker is not Player attacker )
			{
				PlayerDiedRpc( To.Single( victim ), null );
				OnKilledMessage( 0, "", client.SteamId, client.Name, "died" );
				return;
			}
			
			PlayerDiedRpc( To.Single( victim ), attacker );

			// Apply a kill
			var attackerClient = attacker.GetClientOwner();
			attackerClient.SetScore( "kills", attackerClient.GetScore<int>( "kills" ) + 1 );

			// Killstreak tracking
			attacker.CurrentStreak++;

			//
			// Give out medals to the attacker
			//
			List<Medal> medals = Medals.KillMedals.Where( medal => medal.Condition.Invoke( attacker, victim ) ).ToList();

			string[] medalArr = new string[medals.Count];
			for ( int i = 0; i < medals.Count; ++i )
				medalArr[i] = medals[i].Name;

			// Display "YOU FRAGGED" message
			PlayerKilledRpc( To.Single( attacker ), attacker, victim, medalArr );

			OnKilledMessage( attackerClient.SteamId, attackerClient.Name, client.SteamId, client.Name, "Railgun" );
		}

		[ClientRpc]
		public void PlayerRespawnRpc()
		{
			InstagibHud.CurrentHud?.OnRespawn();
		}

		[ServerCmd( "recreatehud", Help = "Recreate hud object" )]
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
			var attackerName = "suicide";
			if ( attacker != null )
				attackerName = attacker.GetClientOwner()?.SteamId.ToString();
			
			Event.Run( "playerDeath", attackerName, Local.Client.SteamId.ToString() );
			InstagibHud.CurrentHud.OnDeath( attacker?.GetClientOwner()?.Name ?? "Yourself" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( Player attacker, Player victim, string[] medals )
		{
			// Attacker, victim
			// Log.Trace( "Player killed rpc" );
			InstagibHud.CurrentHud.OnKilledMessage( attacker, victim, medals );
			Event.Run( "playerKilled", attacker.GetClientOwner().SteamId.ToString(), victim.GetClientOwner().SteamId.ToString() );
		}
	}
}
