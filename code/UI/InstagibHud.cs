using Instagib.UI.Menus;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static Panel TiltingHudPanel;
		public static Panel StaticHudPanel;
		
		public static InstagibHud CurrentHud;

		private static BaseMenu currentMenu;
		
		public InstagibHud()
		{
			if ( IsClient )
			{
				// Show standard hud
				StaticHudPanel = RootPanel.Add.Panel( "staticpanel" );
				StaticHudPanel.StyleSheet.Load( "/Code/UI/MainPanel.scss" );
				StaticHudPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				StaticHudPanel.AddChild<Crosshair>();
				StaticHudPanel.AddChild<ClassicChatBox>();
				StaticHudPanel.AddChild<Hitmarker>();
				StaticHudPanel.AddChild<FragsPanel>();
				StaticHudPanel.AddChild<NameTags>();
				StaticHudPanel.AddChild<KillFeed>();
				// StaticHudPanel.AddChild<WinnerScreen>();

				SetCurrentMenu( new MainMenu() );

				TiltingHudPanel = RootPanel.AddChild<MainPanel>();
				CurrentHud = this;
			}
		}

		public void SetCurrentMenu( BaseMenu menu )
		{
			currentMenu?.Delete();
			currentMenu = menu;
			menu.Parent = StaticHudPanel;
		}

		public void OnDeath( string killer )
		{
			Host.AssertClient();
			
			// We died
			TiltingHudPanel?.DeleteChildren();

			StaticHudPanel.AddChild<DeathsPanel>();
			DeathsPanel.Instance.AddDeathMessage( "Railgun", killer );
		}

		public void OnRespawn()
		{
			Host.AssertClient();
			TiltingHudPanel = RootPanel.AddChild<MainPanel>();
		}

		public void OnKilledMessage( Player attacker, Player victim, string[] medals )
		{
			if ( attacker.Client.SteamId != (Local.Client?.SteamId) )
				return;
			
			// We killed someone
			FragsPanel.Instance.AddFragMessage( "Railgun", victim.Client.Name, medals );
		}
	}
}
