using Instagib.UI;
using Sandbox;

namespace Instagib
{
	[Library( "instagib", Title = "Instagib")]
	public partial class InstagibGame : Sandbox.Game
	{
		public InstagibGame()
		{
			if ( IsServer )
			{
				new InstagibHud();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new InstagibPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}
}
