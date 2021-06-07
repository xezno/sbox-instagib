using System.Collections.Generic;
using System.Linq;
using Instagib.UI;
using Sandbox;

namespace Instagib
{
	[Library( "instagib", Title = "Instagib")]
	public partial class InstagibGame : Sandbox.Game
	{
		private static InstagibHud hud;
		public InstagibGame()
		{
			if ( IsServer )
			{
				hud = new InstagibHud();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new InstagibPlayer();
			client.Pawn = player;

			player.Respawn();
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			base.OnKilled( client, pawn );
			if ( pawn.LastAttacker == null )
				return;
			
			var attacker = pawn.LastAttacker as InstagibPlayer;
			var victim = pawn as InstagibPlayer;
			attacker.CurrentStreak++;
			
			Log.Info( $"Current streak: {attacker.CurrentStreak}" );
			Log.Info( "Checking medals" );
			Log.Info( $"{pawn.LastAttacker.GetClientOwner().Name} receives:");

			List<Medal> medals = new();
			foreach ( var medal in Medals.KillMedals )
			{
				if ( medal.Condition.Invoke( attacker, victim ) )
				{
					Log.Info( $"Medal: {medal.Name}\t{medal.Description}" );
					medals.Add( medal );
				}
			}

			string[] medalArr = new string[ medals.Count ];
			for ( int i = 0; i < medals.Count; ++i )
				medalArr[i] = medals[i].Name;
			
			PlayerKilledRpc( To.Single( attacker ), attacker, victim, medalArr );
		}

		[ServerCmd( "recreatehud", Help = "Recreate hud object" )]
		public static void RecreateHud()
		{
			hud.Delete();
			hud = new();
			Log.Info( "Recreated HUD" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( InstagibPlayer attacker, InstagibPlayer victim, string[] medals )
		{
			InstagibHud.CurrentHud.OnKilledMessage( attacker, victim, medals );
		}
	}
}
