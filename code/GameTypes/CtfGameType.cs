using Sandbox;
using Instagib.Teams;

namespace Instagib.GameTypes
{
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

			RedTeam.TeamName = "Red";
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
	}
}
