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
		
		[ServerCmd( "recreatehud", Help = "Recreate hud object" )]
		public static void RecreateHud()
		{
			hud.Delete();
			hud = new();
			Log.Info( "Recreated HUD" );
		}
	}
}
