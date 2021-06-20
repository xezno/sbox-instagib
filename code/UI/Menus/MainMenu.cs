using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class MainMenu : Menu
	{
		public Slider FovSlider { get; set; }
		public Slider TestSlider { get; set; }
		
		public MainMenu()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.OnValueChange += SetFov;
			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => 70f.LerpTo( 110f, value ).CeilToInt();

			TestSlider.OnValueChange += f => FovSlider.SnapRate = f;
			TestSlider.SnapRate = 1;
			TestSlider.ValueCalcFunc = value => 1f.LerpTo( 5, value ).CeilToInt();
		}

		public void Close()
		{
			Delete();
		}

		public void SetFov( int newFov )
		{
			( Local.Pawn.Camera as FirstPersonCamera ).defaultFov = (int)newFov;
		}
	}
}
