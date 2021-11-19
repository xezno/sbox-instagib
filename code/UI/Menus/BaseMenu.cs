using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class BaseMenu : Menu
	{
		public BaseMenu() : base()
		{
			AddClass( "menu" );
			StyleSheet.Load( "/Code/UI/Menus/BaseMenu.scss" );
		}

		public override void Tick()
		{
			base.Tick();
			SetClass( "open", Input.Down( InputButton.Score ) );
		}

		public void Toggle()
		{
			InstagibHud.currentHud.SetCurrentMenu( new MainMenu() );
		}

		public void ShowSettings()
		{
			InstagibHud.currentHud.SetCurrentMenu( new SettingsMenu() );
		}
	}
}
