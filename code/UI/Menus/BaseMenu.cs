using Instagib.UI.Elements;
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

		public void Toggle()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( new MainMenu() );
		}
	}
}
