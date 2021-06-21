using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static Crosshair Instance { get; set; }
		
		public Crosshair()
		{
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
			Instance = this;
			
			AddClass( "crosshair" );
			if ( PlayerSettings.CrosshairVisible )
				AddClass( "visible" );
			
			SetText( PlayerSettings.CrosshairGlyph );
			
			StyleSheet.Parse( $"crosshair {{ font-size: {PlayerSettings.CrosshairSize}px; }}" );
		}
	}
}
