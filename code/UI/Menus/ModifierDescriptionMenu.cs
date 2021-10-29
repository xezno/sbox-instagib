using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Menus
{
	public class ModifierDescriptionMenu : BaseMenu
	{
		public Panel MenuItems { get; set; }
		public ModifierDescriptionMenu( ModifierInfo info ) : base()
		{
			SetTemplate( "/Code/UI/Menus/ModifierDescriptionMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/ModifierDescriptionMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			MenuItems.Add.Label( info.Name, "title" );
			MenuItems.Add.Label( $"Published by {info.Author}", "subtitle" );
			MenuItems.Add.Label( info.Description, "description" );
			MenuItems.Add.Label( info.ModifierText, "info" );
		}
	}
}
