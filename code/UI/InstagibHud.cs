using Instagib.UI.Menus;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static Panel TiltingHudPanel;
		public static Panel StaticHudPanel;
		private SettingsMenu settingsMenu;
		
		public static InstagibHud CurrentHud;

		private bool menuVisible = false;
		
		public InstagibHud()
		{
			if ( IsClient )
			{
				ToggleMenu( false );
				
				CurrentHud = this;
			}
		}

		public static void ToggleMainMenu() => CurrentHud.ToggleMenu();

		private void ToggleMenu( bool? forceState = null )
		{
			void HideMenu()
			{
				settingsMenu?.RestoreSettings();
				settingsMenu?.Delete();
				
				StaticHudPanel = RootPanel.Add.Panel( "staticpanel" );
				StaticHudPanel.StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
				StaticHudPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				StaticHudPanel.AddChild<Crosshair>();
				StaticHudPanel.AddChild<ClassicChatBox>();
				StaticHudPanel.AddChild<Hitmarker>();
				
				TiltingHudPanel = RootPanel.AddChild<MainPanel>();
			}
			
			void ShowMenu()
			{
				StaticHudPanel?.Delete();
				TiltingHudPanel?.Delete();
				
				settingsMenu = RootPanel.AddChild<SettingsMenu>();
			}

			if ( forceState != null )
				menuVisible = forceState.Value;
			else
				menuVisible = !menuVisible;
			
			if ( menuVisible ) 
				ShowMenu();
			else
				HideMenu();
		}

		public void OnKilledMessage( Player attacker, Player victim, string[] medals )
		{
			if ( attacker.GetClientOwner().SteamId != (Local.Client?.SteamId) )
				return;

			var fragMessage = new FragMessage( victim.GetClientOwner().Name, medals );
			fragMessage.Parent = RootPanel;
		}

		[Event.Tick.Client]
		public void OnTick()
		{
			if ( Input.Pressed( InputButton.Menu ) )
			{
				Log.Trace( "Toggling menu" );
				ToggleMenu();
			}
		}
	}
}
