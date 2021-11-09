using Sandbox;

namespace Instagib.GameTypes
{
	public partial class BaseGameType : BaseNetworkable
	{
		[Net] public string GameTypeName { get; set; }
		[Net] public string GameTypeDescription { get; set; }
		[Net] public bool IsExperimental { get; set; }

		public string LibraryName { get; set; }
		
		public virtual bool GameShouldEnd()
		{
			return false;
		}

		public virtual void AssignPlayerTeam( Player player )
		{
			player.Team = new Teams.BaseTeam()
			{
				TeamName = player.Client.Name,
				TeamColor = "#deadbeef",
				TeamId = player.NetworkIdent
			};
		}
	}
}
