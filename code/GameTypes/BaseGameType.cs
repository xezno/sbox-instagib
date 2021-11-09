using Sandbox;

namespace Instagib.GameTypes
{
	public class BaseGameType : BaseNetworkable
	{
		public string GameTypeName { get; protected set; }
		
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
