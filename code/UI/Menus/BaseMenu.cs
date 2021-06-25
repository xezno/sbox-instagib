using Sandbox.UI;

namespace instagib.UI.Menus
{
	public class BaseMenu : Menu
	{
		public BaseMenu() : base()
		{
			AddClass( "menu" );
			StyleSheet.Load( "/Code/UI/Menus/BaseMenu.scss" );
		}
	}
}
