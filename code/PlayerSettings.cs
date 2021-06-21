using System.Security;
using Sandbox;

namespace Instagib
{
	public static class PlayerSettings
	{
		public static float Fov { get; set; } = 90;
		public static float ViewmodelOffset { get; set; } = 0;
		public static bool ViewmodelVisible { get; set; } = true;
		public static bool CrosshairVisible { get; set; } = true;
		public static bool ViewmodelFlip { get; set; } = false;
		public static string CrosshairGlyph { get; set; } = "a";

		public static void Load()
		{
			Host.AssertClient();
			
			Fov = Cookie.Get<float>( "Instagib.Fov", 100 );
			ViewmodelOffset = Cookie.Get<float>( "Instagib.ViewmodelOffset", 0 );
			ViewmodelVisible = Cookie.Get( "Instagib.ViewmodelVisible", true );
			CrosshairVisible = Cookie.Get( "Instagib.CrosshairVisible", true );
			ViewmodelFlip = Cookie.Get( "Instagib.ViewmodelFlip", false );
			CrosshairGlyph = Cookie.Get( "Instagib.CrosshairGlyph", "t" );
		}

		public static void Save()
		{
			Host.AssertClient();
			
			Cookie.Set( "Instagib.Fov", Fov );
			Cookie.Set( "Instagib.ViewmodelOffset", ViewmodelOffset );
			Cookie.Set( "Instagib.ViewmodelVisible", ViewmodelVisible );
			Cookie.Set( "Instagib.CrosshairVisible", CrosshairVisible );
			Cookie.Set( "Instagib.ViewmodelFlip", ViewmodelFlip );
			Cookie.Set( "Instagib.CrosshairGlyph", CrosshairGlyph );
		}
	}
}
