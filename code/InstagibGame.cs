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
			Precache.Add( "particles/gib_blood.vpcf" );
			Precache.Add( "sounds/jump.vsnd" );
			
			Precache.Add( "weapons/railgun/particles/railgun_beam.vpcf" );
			Precache.Add( "weapons/railgun/particles/railgun_pulse.vpcf" );
			Precache.Add( "weapons/railgun/sounds/railgun_fire.vsnd" );
			
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

			if ( pawn.LastAttacker is not InstagibPlayer attacker )
				return;
			
			if ( pawn is not InstagibPlayer victim )
				return;
			
			attacker.CurrentStreak++;
			
			List<Medal> medals = Medals.KillMedals.Where( medal => medal.Condition.Invoke( attacker, victim ) ).ToList();

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
