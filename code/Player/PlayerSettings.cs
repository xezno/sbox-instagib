using System.Security;
using Sandbox;

namespace Instagib
{
	public static class PlayerSettings
	{
		public static float ZoomedFov { get; set; } = 60;
		public static float Fov { get; set; } = 90;

		public static float ViewmodelOffset { get; set; } = 0;
		public static bool ViewmodelVisible { get; set; } = true;
		public static bool ViewmodelFlip { get; set; } = false;
		public static Color CrosshairColor { get; set; } = Color.Green;
		public static float ViewTiltMultiplier { get; set; } = 1.0f;

		public static Color EnemyOutlineColor { get; set; } = Color.Red;

		public static void Load()
		{
			Host.AssertClient();
			
			Fov = Cookie.Get<float>( "Instagib.Fov", 100 );
			ViewmodelOffset = Cookie.Get<float>( "Instagib.ViewmodelOffset", 0 );
			ViewmodelVisible = Cookie.Get( "Instagib.ViewmodelVisible", true );
			ViewmodelFlip = Cookie.Get( "Instagib.ViewmodelFlip", false );
			ViewTiltMultiplier = Cookie.Get( "Instagib.ViewTiltMultiplier", 1.0f );
			CrosshairColor = Color.Parse( Cookie.Get( "Instagib.CrosshairColor", Color.Red.Hex ) ) ?? Color.Green;
			EnemyOutlineColor = Color.Parse( Cookie.Get( "Instagib.EnemyOutlineColor", Color.Red.Hex ) ) ?? Color.Red;
		}

		public static void Save()
		{
			Host.AssertClient();
			
			Cookie.Set( "Instagib.Fov", Fov );
			Cookie.Set( "Instagib.ViewmodelOffset", ViewmodelOffset );
			Cookie.Set( "Instagib.ViewmodelVisible", ViewmodelVisible );
			Cookie.Set( "Instagib.ViewmodelFlip", ViewmodelFlip );
			Cookie.Set( "Instagib.ViewTiltMultiplier", ViewTiltMultiplier );
			Cookie.Set( "Instagib.CrosshairColor", CrosshairColor.Hex );
			Cookie.Set( "Instagib.EnemyOutlineColor", EnemyOutlineColor.Hex );
		}
	}
}
