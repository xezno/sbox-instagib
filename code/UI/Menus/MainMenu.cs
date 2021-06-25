using instagib.UI.Menus;
using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class MainMenu : Menu
	{
		public Panel Scroll { get; set; }

		public MainMenu()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			// Add scrollbar
			var scrollbar = AddChild<Scrollbar>();
			scrollbar.Panel = Scroll;
		}
	}
}
