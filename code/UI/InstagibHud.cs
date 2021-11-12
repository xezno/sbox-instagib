using Instagib.UI.Menus;
using Instagib.UI.PostGameScreens;
using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public partial class InstagibHud : Sandbox.HudEntity<RootPanel>
	{
		public static Panel TiltingHudPanel;
		public static Panel StaticHudPanel;

		private static Panel WinnerScreen;
		private static Panel MapVoteScreen;

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
				StaticHudPanel.AddChild<MessagesPanel>();
				StaticHudPanel.AddChild<NameTags>();
				StaticHudPanel.AddChild<KillFeed>();

				SetCurrentMenu( new MainMenu() );

				TiltingHudPanel = RootPanel.AddChild<MainPanel>();
				CurrentHud = this;
			}
		}

		bool isFirstTick = true;

		[Event.Tick.Client]
		public void OnTick()
		{
			if ( isFirstTick )
			{
				Game.Instance.GameType.CreateHUDElements( TiltingHudPanel, StaticHudPanel );
				isFirstTick = false;
			}
		}

		public static void ToggleMapVoteScreen( bool oldValue, bool newValue )
		{
			if ( newValue )
				MapVoteScreen = StaticHudPanel.AddChild<MapVoteScreen>();
			else
				MapVoteScreen?.Delete();
		}

		public static void ToggleWinnerScreen( bool oldValue, bool newValue )
		{
			if ( newValue )
				WinnerScreen = StaticHudPanel.AddChild<WinnerScreen>();
			else
				WinnerScreen?.Delete();
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

			MessagesPanel.Instance.AddDeathMessage( "Railgun", killer );
		}

		public void OnRespawn()
		{
			Host.AssertClient();
			TiltingHudPanel = RootPanel.AddChild<MainPanel>();
			isFirstTick = true;
		}

		public void OnKilledMessage( Player attacker, Player victim, string[] medals )
		{
			if ( attacker.Client.SteamId != (Local.Client?.SteamId) )
				return;

			// We killed someone
			MessagesPanel.Instance.AddFragMessage( "Railgun", victim.Client.Name, medals );
		}
	}
}
