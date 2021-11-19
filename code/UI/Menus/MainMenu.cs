using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class MainMenu : BaseMenu
	{
		public MainMenu() : base()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason
		}
	}
}
