﻿using Instagib.UI.Menus;
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
			InstagibHud.CurrentHud.SetCurrentMenu( new CustomizeMenu() );
		}

		public void ShowProfile()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( new ProfileMenu() );
		}

		public void ShowShop()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( new ShopMenu() );
		}
	}
}
