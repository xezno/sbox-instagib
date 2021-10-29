using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Menus
{
	public class ModifiersMenu : BaseMenu
	{
		public Panel ModifierList { get; set; }

		public ModifiersMenu() : base()
		{
			SetTemplate( "/Code/UI/Menus/ModifiersMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/ModifiersMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			for ( int i = 0; i < 10; ++i )
			{
				var modifier = new ModifierInfo
				{
					Name = "Moon Melee",
					Description = "Low-gravity melee action",
					ModifierText = "Gravity: 400\nWeapon: Melee-only\nTime limit: 2 minutes",
					Image = "https://lumiere-a.akamaihd.net/v1/images/Death-Star-I-copy_36ad2500.jpeg",
					Author = "Alex"
				};

				var modifierElement = new ModifierElement( modifier );
				modifierElement.Parent = ModifierList;
			}
		}
	}

	public struct ModifierInfo
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ModifierText { get; set; }
		public string Image { get; set; }
		public string Author { get; set; }
	}

	public class ModifierElement : Panel
	{
		public ModifierElement( ModifierInfo info )
		{
			Add.Label( info.Name, "name" );
			Add.Label( info.Description, "description" );
			Add.Label( info.ModifierText, "info" );
			Add.Label( $"Published by {info.Author}", "author" );
			Add.Image( info.Image, "background" );

			AddEventListener( "onclick", () =>
			{
				InstagibHud.CurrentHud.SetCurrentMenu( new ModifierDescriptionMenu( info ) );
			} );
		}
	}
}
