using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static Crosshair Instance { get; set; }
		
		public Crosshair()
		{
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			Instance = this;
			
			AddClass( "crosshair" );
			if ( PlayerSettings.CrosshairVisible )
				AddClass( "visible" );
			
			SetText( PlayerSettings.CrosshairGlyph );
			
			StyleSheet.Parse( $"crosshair {{ width: {PlayerSettings.CrosshairSize}px; height: {PlayerSettings.CrosshairSize}px; font-size: {PlayerSettings.CrosshairSize}px; }}" );
		}
	}
}
