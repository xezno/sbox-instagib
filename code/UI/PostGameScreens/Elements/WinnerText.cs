using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.PostGameScreens.Elements
{
	public class WinnerText : Panel
	{
		public WinnerText( string position, Client player, string className )
		{
			Add.Label( position, "position " + className );
			Add.Label( player.Name, "player " + className );

			var kdrString = $"K: {player.GetInt( "kills" )} | D: {player.GetInt( "deaths" )}";
			Add.Label( kdrString, "stats " + className );
		}
	}
}
