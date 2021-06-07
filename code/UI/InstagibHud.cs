using Instagib.UI;
using Instagib.UI.Menus;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static Panel CurrentHudPanel;
		public static InstagibHud CurrentHud;
		
		public InstagibHud()
		{
			if ( IsClient )
			{
				// RootPanel.AddChild<MainMenu>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				RootPanel.AddChild<Crosshair>();
				
				var mainPanel = RootPanel.AddChild<MainPanel>();
				mainPanel.AddChild<KillFeed>();
				
				CurrentHudPanel = mainPanel;
				CurrentHud = this;
			}
		}

		public void OnKilledMessage( InstagibPlayer attacker, InstagibPlayer victim, string[] medals )
		{
			if ( attacker.GetClientOwner().SteamId != (Local.Client?.SteamId) )
				return;

			var fragMessage = new FragMessage( victim.GetClientOwner().Name, medals );
			fragMessage.Parent = RootPanel;
		}
	}
}
