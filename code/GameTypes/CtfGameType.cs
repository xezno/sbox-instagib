using Sandbox;
using Instagib.Teams;
using Sandbox.UI;
using Instagib.UI.Elements;
using Instagib.UI.PostGameScreens;
using System.Collections.Generic;
using Sandbox.UI.Construct;

namespace Instagib.GameTypes
{
	[Library( "gametype_ctf", Title = "Capture the flag", Description = "Capture the enemy's flag to win" )]
	public partial class CtfGameType : BaseGameType
	{
		[Net] public BaseTeam BlueTeam { get; set; }
		[Net] public BaseTeam RedTeam { get; set; }

		[Net] public int BlueCaptures { get; set; }
		[Net] public int RedCaptures { get; set; }

		public CtfGameType()
		{
			GameTypeName = "Capture the flag";
			GameTypeDescription = "Capture the enemy's flag to win";
			IsExperimental = true;

			BlueTeam = new()
			{
				TeamName = "Blue",
				TeamId = 0,
				TeamColor = "#71a5fe"
			};

			RedTeam = new()
			{
				TeamName = "Red",
				TeamId = 1,
				TeamColor = "#fe7171"
			};
		}

		public void CaptureFlag( BaseTeam team )
		{
			if ( team == BlueTeam )
			{
				BlueCaptures++;
			}
			else
			{
				RedCaptures++;
			}
		}

		public override void CreateWinnerElements( WinnerScreen winnerScreen )
		{
			var sortedClients = new List<Client>( Client.All );
			sortedClients.Sort( InstagibGlobal.SortClients );
			var playersPanel = winnerScreen.Add.Panel( "players" );
			int particleCount = 0;

			BaseTeam winningTeam = null;
			if ( RedCaptures > BlueCaptures )
				winningTeam = RedTeam;
			else if ( BlueCaptures > RedCaptures )
				winningTeam = BlueTeam;

			if ( winningTeam != null )
				winnerScreen.Add.Label( $"{winningTeam.TeamName.ToUpper()} WINS", "title " + winningTeam.TeamName );
			else
				winnerScreen.Add.Label( $"TIE", "title" );

			{
				var redTeam = playersPanel.Add.Panel( "red-team" );
				redTeam.Add.Label( "RED", "team-name red" );
				redTeam.Add.Label( RedCaptures.ToString(), "captures red" );
			}
			{
				var blueTeam = playersPanel.Add.Panel( "blue-team" );
				blueTeam.Add.Label( "BLUE", "team-name blue" );
				blueTeam.Add.Label( BlueCaptures.ToString(), "captures blue" );
			}

			particleCount = Local.Client.GetTeam() == winningTeam ? 64 : 0;
			winnerScreen.CreateWinnerParticles( particleCount );
		}

		public override bool GameShouldEnd()
		{
			return RedCaptures >= 3 || BlueCaptures >= 3;
		}

		public override void AssignPlayerTeam( Player player )
		{
			var teamIndex = Client.All.Count % 2;
			var selectedTeam = BlueTeam;
			if ( teamIndex != 0 )
				selectedTeam = RedTeam;

			player.Team = selectedTeam.Clone();
			Log.Trace( $"Added player {player.Name} to team {selectedTeam.TeamName}" );
		}

		public override void CreateHUDElements( Panel RootPanel, Panel StaticHudPanel )
		{
			base.CreateHUDElements( RootPanel, StaticHudPanel );

			RootPanel.AddChild<FlagInfo>();

			StaticHudPanel.AddChild<FlagTags>();
		}
	}
}
