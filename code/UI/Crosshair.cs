using System.Runtime.ExceptionServices;
using Sandbox;
using Sandbox.Rcon;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static bool visible;
		public static bool Visible
		{
			get => visible;
			set
			{
				visible = value;
				if ( visible )
					Instance.AddClass( "visible" );
				else
					Instance.RemoveClass( "visible" );
			}
		}

		public static string Glyph;

		private static Crosshair Instance { get; set; }
		
		public Crosshair()
		{
			Instance = this;
			Visible = true;
			
			AddClass( "crosshair" );
			AddClass( "visible" );
			SetText( Glyph );
			
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
		}

		[ClientCmd( "crosshair_glyph" )]
		public static void SetCrosshair( string character )
		{
			if ( character.Length != 1 )
				return;

			Glyph = character;
			Instance.SetText( Glyph );
		}
	}
}
