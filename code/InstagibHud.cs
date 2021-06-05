using Instagib.UI;
using Instagib.UI.Menus;
using Sandbox;
using Sandbox.UI;

namespace Instagib
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static Panel CurrentHudPanel;
		
		public InstagibHud()
		{
			if ( IsClient )
			{
				// RootPanel.AddChild<MainMenu>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				RootPanel.AddChild<Crosshair>();
				
				var mainPanel = RootPanel.AddChild<MainPanel>();
				mainPanel.AddChild<KillFeed>();

				var fragMessage = new FragMessage( "big boy" );
				fragMessage.Parent = mainPanel;

				CurrentHudPanel = mainPanel;
			}
		}
	}

}
