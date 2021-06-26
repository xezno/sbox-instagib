using Instagib.UI.Menus;
using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;
using System;

namespace Instagib.UI.Menus
{
	public class MainMenu : BaseMenu
	{
		public MainMenu() : base()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason
		}

		public void ShowSettings()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( new SettingsMenu() );
		}

		public void HideMenu()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( null );
		}

		public void ShowCustomize()
		{
			throw new NotImplementedException();
		}

		public void ShowProfile()
		{
			throw new NotImplementedException();
		}

		public void ShowShop()
		{
			throw new NotImplementedException();
		}

		public void ShowBugReport()
		{
			throw new NotImplementedException();
		}
	}
}
