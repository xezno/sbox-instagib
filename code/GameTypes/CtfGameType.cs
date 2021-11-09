using Sandbox;
using Instagib.Teams;

namespace Instagib.GameTypes
{
	public partial class CtfGameType : BaseNetworkable
	{
		[Net] public BaseTeam TeamA { get; set; }
		[Net] public BaseTeam TeamB { get; set; }

		public CtfGameType()
		{
			TeamA = new()
			{
				TeamName = "Blue",
				TeamId = 0,
				TeamColor = "#71a5fe"
			};

			TeamB = new()
			{
				TeamName = "Red",
				TeamId = 1,
				TeamColor = "#fe7171"
			};

			TeamB.TeamName = "Red";
		}

		public void AssignPlayerTeam( Player player )
		{
			var teamIndex = Client.All.Count % 2;
			var selectedTeam = TeamA;
			if ( teamIndex != 0 )
				selectedTeam = TeamB;

			player.Team = selectedTeam.Clone();
			Log.Trace( $"Added player {player.Name} to team {selectedTeam.TeamName}" );
		}
	}
}
