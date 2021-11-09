using System.Collections.Generic;
using System.Threading.Tasks;
using Instagib.UI;
using Sandbox;

namespace Instagib
{
	public partial class Game : Sandbox.Game
	{
		private static InstagibHud hud;
		public static Game Instance;

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
			}

			Instance = this;
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );
			CurrentState.OnPlayerJoin( cl );

			Log.Trace( $"Lobby name: {Global.Lobby.Title}" );

			var player = new Player( cl );
			cl.Pawn = player;
			player.Respawn();
		}
		
		public override void DoPlayerNoclip( Client cl )
		{
			if ( cl.SteamId != InstagibGlobal.AlexSteamId )
				return;

			base.DoPlayerNoclip( cl );
		}

		public override void DoPlayerDevCam( Client cl )
		{
			if ( cl.SteamId != InstagibGlobal.AlexSteamId )
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
			InstagibHud.CurrentHud.OnDeath( attacker?.Client?.Name ?? "Yourself" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( Player attacker, Player victim, string[] medals )
		{
			// Attacker, victim
			InstagibHud.CurrentHud.OnKilledMessage( attacker, victim, medals );
		}
	}
}
