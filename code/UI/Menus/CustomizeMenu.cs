using Sandbox.UI;

namespace Instagib.UI.Menus
{
	[UseTemplate]
	public class CustomizeMenu : BaseMenu
	{
		public CustomizeMenu()
		{
			// Railgun renderer
			var railgunRender = AddChild<RailgunRender>();
		}
	}
}
