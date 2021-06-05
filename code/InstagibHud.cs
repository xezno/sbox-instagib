using Instagib.UI.Menus;
using Sandbox.UI;

namespace Instagib
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static RootPanel Current;
		
		public InstagibHud()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/InstagibHud.html" );
				
				// RootPanel.AddChild<MainMenu>();
				RootPanel.AddChild<KillFeed>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();

				Current = RootPanel;
			}
		}
	}

}
