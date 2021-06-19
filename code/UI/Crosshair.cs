using System.Runtime.ExceptionServices;
using Sandbox;
using Sandbox.Rcon;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static Crosshair Instance { get; set; }
		
		public Crosshair()
		{
			Instance = this;
			
			SetClass( "crosshair", true );
			SetText( "a" );
			
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
		}

		[ClientCmd( "crosshair_glyph" )]
		public static void SetCrosshair( string character )
		{
			if ( character.Length != 1 )
				return;
			
			Instance.SetText( character );
		}
	}
}
