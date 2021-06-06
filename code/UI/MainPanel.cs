using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		public string PlayerHealth => $"{Local.Client.Pawn.Health}";
		public string PlayerSpeed => $"{Local.Client.Pawn.Velocity.WithZ( 0 ).Length:N0}u/s";

		public MainPanel()
		{
			SetClass( "mainpanel", true );
			SetTemplate( "/InstagibHud.html" );
		}
	}
}
