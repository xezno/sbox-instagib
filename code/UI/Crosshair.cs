using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		int fireCounter;

		public Crosshair()
		{
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}
	}
}
