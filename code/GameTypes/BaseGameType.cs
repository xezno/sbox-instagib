using Instagib.UI.PostGameScreens;
using Sandbox;
using Sandbox.UI;

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
				TeamColor = "#bada55",
				TeamId = player.NetworkIdent
			};
		}

		public virtual void Notify( string str )
		{
			// Shouldn't be using the chatbox for this
			// TODO: revisit
			ClassicChatBox.AddInformation( To.Everyone, str, null, true );
		}

		public virtual void CreateWinnerElements( WinnerScreen winnerScreen ) { }

		public virtual void CreateHUDElements( Panel panel, Panel StaticHudPanel ) { }
	}
}
