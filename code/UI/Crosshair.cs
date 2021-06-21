using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static Crosshair Instance { get; set; }
		
		public Crosshair()
		{
			Instance = this;
			
			AddClass( "crosshair" );
			if ( PlayerSettings.CrosshairVisible )
				AddClass( "visible" );
			
			SetText( PlayerSettings.CrosshairGlyph );
			
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
		}
	}
}
