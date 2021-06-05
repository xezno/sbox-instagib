using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Panel
	{
		public Crosshair()
		{
			SetClass( "crosshair", true );
			AddChild<Panel>().SetClass( "crosshair-inner", true );
			StyleSheet.Load( "/InstagibHud.scss" );
		}
	}
}
