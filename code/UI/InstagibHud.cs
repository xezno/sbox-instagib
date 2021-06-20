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
				RootPanel.AddChild<MainMenu>();

				return;
				
				//
				// Stuff that doesn't move / tilt / etc.
				//
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				RootPanel.AddChild<Crosshair>();
				RootPanel.AddChild<ClassicChatBox>();
				RootPanel.AddChild<Hitmarker>();
				
				//
				// Stuff that moves / tilts / etc.
				//
				var mainPanel = RootPanel.AddChild<MainPanel>();
				
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
