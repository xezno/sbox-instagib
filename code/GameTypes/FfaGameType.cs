using Instagib.UI.PostGameScreens;
using Sandbox;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Instagib.GameTypes
{
	[Library( "gametype_ffa", Title = "Free for all", Description = "Basic deathmatch game type" )]
	public partial class FfaGameType : BaseGameType
	{
		public FfaGameType()
		{
			GameTypeName = "Free-for-all";
			GameTypeDescription = "";
			IsExperimental = false;
		}

		private static string GetClassForPosition( int position )
		{
			switch ( position )
			{
				case 1:
					return "first";
				case 2:
					return "second";
				case 3:
					return "third";
			}
			return "";
		}

		public override void CreateWinnerElements( WinnerScreen winnerScreen )
		{
			var sortedClients = new List<Client>( Client.All );
			sortedClients.Sort( InstagibGlobal.SortClients );
			var playersPanel = winnerScreen.Add.Panel( "players" );
			int particleCount = 0;

			//
			// Local player position
			//
			for ( int i = 0; i < sortedClients.Count; i++ )
			{
				Client client = sortedClients[i];
				if ( client == Local.Client )
				{
					winnerScreen.Add.Label( $"YOU PLACED #{i + 1}", "title " + GetClassForPosition( i + 1 ) );

					particleCount = (3 - i) * 16;
				}
			}

			//
			// Other players
			//
			for ( int i = 0; i < sortedClients.Count; i++ )
			{
				Client client = sortedClients[i];
				switch ( i )
				{
					case 0:
						var firstPlace = new WinnerText( "#1", client, "first" );
						firstPlace.Parent = playersPanel;
						break;
					case 1:
						var secondPlace = new WinnerText( "#2", client, "second" );
						secondPlace.Parent = playersPanel;
						break;
					case 2:
						var thirdPlace = new WinnerText( "#3", client, "third" );
						thirdPlace.Parent = playersPanel;
						break;
				}
			}

			winnerScreen.CreateWinnerParticles( particleCount );
		}
	}
}
