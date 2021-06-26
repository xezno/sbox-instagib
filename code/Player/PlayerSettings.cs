using System.Security;
using Sandbox;

namespace Instagib
{
	public static class PlayerSettings
	{
		public static float Fov { get; set; } = 90;

		public static float ViewmodelOffset { get; set; } = 0;
		public static bool ViewmodelVisible { get; set; } = true;
		public static bool ViewmodelFlip { get; set; } = false;

		public static bool CrosshairVisible { get; set; } = true;
		public static string CrosshairGlyph { get; set; } = "a";
		public static int CrosshairSize { get; set; } = 24;

		public static Color EnemyOutlineColor { get; set; } = Color.Red;

		public static void Load()
		{
			Host.AssertClient();
			
			Fov = Cookie.Get<float>( "Instagib.Fov", 100 );
			ViewmodelOffset = Cookie.Get<float>( "Instagib.ViewmodelOffset", 0 );
			ViewmodelVisible = Cookie.Get( "Instagib.ViewmodelVisible", true );
			ViewmodelFlip = Cookie.Get( "Instagib.ViewmodelFlip", false );
			CrosshairVisible = Cookie.Get( "Instagib.CrosshairVisible", true );
			CrosshairGlyph = Cookie.Get( "Instagib.CrosshairGlyph", "t" );
			CrosshairSize = Cookie.Get( "Instagib.CrosshairSize", 24 );
			EnemyOutlineColor = Color.Parse( Cookie.Get( "Instagib.EnemyOutlineColor", Color.Red.Hex ) ) ?? Color.Red;
		}

		public static void Save()
		{
			Host.AssertClient();
			
			Cookie.Set( "Instagib.Fov", Fov );
			Cookie.Set( "Instagib.ViewmodelOffset", ViewmodelOffset );
			Cookie.Set( "Instagib.ViewmodelVisible", ViewmodelVisible );
			Cookie.Set( "Instagib.ViewmodelFlip", ViewmodelFlip );
			Cookie.Set( "Instagib.CrosshairVisible", CrosshairVisible );
			Cookie.Set( "Instagib.CrosshairGlyph", CrosshairGlyph );
			Cookie.Set( "Instagib.CrosshairSize", CrosshairSize );
			Cookie.Set( "Instagib.EnemyOutlineColor", EnemyOutlineColor.Hex );
		}
	}
}
