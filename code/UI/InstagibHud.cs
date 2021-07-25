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
			StaticHudPanel?.Delete();
			TiltingHudPanel?.Delete();

			if ( newMenu == null )
			{
				// Show standard hud
				StaticHudPanel = RootPanel.Add.Panel( "staticpanel" );
				StaticHudPanel.StyleSheet.Load( "/Code/UI/MainPanel.scss" );
				StaticHudPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				StaticHudPanel.AddChild<Crosshair>();
				StaticHudPanel.AddChild<ClassicChatBox>();
				StaticHudPanel.AddChild<Hitmarker>();
				StaticHudPanel.AddChild<FragsPanel>();

				TiltingHudPanel = RootPanel.AddChild<MainPanel>();
			}
			else
			{
				newMenu.Parent = RootPanel;
				currentMenu = newMenu;
			}
		}

		public void OnDeath( string killer )
		{
			Host.AssertClient();
			// Log.Trace( "HUD: Death" );
			
			// We died
			StaticHudPanel?.DeleteChildren();
			TiltingHudPanel?.DeleteChildren();

			StaticHudPanel.AddChild<DeathsPanel>();
			DeathsPanel.Instance.AddDeathMessage( "Railgun", killer );
		}

		public void OnRespawn()
		{
			RootPanel.DeleteChildren();
			Host.AssertClient();
			// Log.Trace( "HUD: Respawn" );
			SetCurrentMenu( null );
		}

		public void OnKilledMessage( Player attacker, Player victim, string[] medals )
		{
			if ( attacker.GetClientOwner().SteamId != (Local.Client?.SteamId) )
				return;

			// Log.Trace( "Killed someone" );
			
			// We killed someone
			FragsPanel.Instance.AddFragMessage( "Railgun", victim.GetClientOwner().Name, medals );
		}

		[Event.Tick.Client]
		public void OnTick()
		{
			if ( Input.Pressed( InputButton.Menu ) )
			{
				// Log.Trace( "Toggling menu" );
				if ( currentMenu is MainMenu )
					SetCurrentMenu( null );
				else if ( Local.Pawn.Velocity.Cross( Vector3.Up ).Length < 30f )
					// Prevent people from accidentally opening the menu
					SetCurrentMenu( new MainMenu() );
			}
		}
	}
}
