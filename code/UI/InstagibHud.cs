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
				SetCurrentMenu( null );

				CurrentHud = this;
			}
		}

		private BaseMenu currentMenu;
		public void SetCurrentMenu( BaseMenu newMenu = null )
		{
			currentMenu?.Delete();
			currentMenu = null;

			if ( newMenu == null )
			{
				// Show standard hud
				StaticHudPanel = RootPanel.Add.Panel( "staticpanel" );
				StaticHudPanel.StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
				StaticHudPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				StaticHudPanel.AddChild<Crosshair>();
				StaticHudPanel.AddChild<ClassicChatBox>();
				StaticHudPanel.AddChild<Hitmarker>();

				TiltingHudPanel = RootPanel.AddChild<MainPanel>();
			}
			else
			{
				StaticHudPanel?.Delete();
				TiltingHudPanel?.Delete();

				newMenu.Parent = RootPanel;
				currentMenu = newMenu;
			}
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
				// Prevent people from accidentally opening the menu
				if ( Local.Pawn.Velocity.Cross( Vector3.Up ).Length > 30f )
					return;

				Log.Trace( "Toggling menu" );
				if ( currentMenu is MainMenu )
					SetCurrentMenu( null );
				else
					SetCurrentMenu( new MainMenu() );
			}
		}
	}
}
