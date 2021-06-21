using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class MainMenu : Menu
	{
		public Slider FovSlider { get; set; }
		public Slider ViewmodelSlider { get; set; }
		public Checkbox ViewmodelToggle { get; set; }
		public Checkbox ViewmodelFlip { get; set; }
		public Checkbox CrosshairToggle { get; set; }
		public TextEntry CrosshairGlyph { get; set; }

		// TODO: Range struct
		private (float, float) fovRange = (70f, 130f);
		private (float, float) viewmodelRange = (0f, 10f);

		public MainMenu()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => fovRange.Item1.LerpTo( fovRange.Item2, value ).CeilToInt();

			FovSlider.Value = 110f;

			ViewmodelSlider.SnapRate = 1;
			ViewmodelSlider.ValueCalcFunc =
				value => viewmodelRange.Item1.LerpTo( viewmodelRange.Item2, value ).CeilToInt();

			// Apply loaded settings
			PlayerSettings.Load();
			FovSlider.Value = ((float)PlayerSettings.Fov).LerpInverse( fovRange.Item1, fovRange.Item2 );
			ViewmodelSlider.Value = PlayerSettings.ViewmodelOffset.LerpInverse( viewmodelRange.Item1, viewmodelRange.Item2 );
			ViewmodelToggle.Value = PlayerSettings.ViewmodelVisible;
			CrosshairToggle.Value = PlayerSettings.CrosshairVisible;
			ViewmodelFlip.Value = PlayerSettings.ViewmodelFlip;
			CrosshairGlyph.Text = PlayerSettings.CrosshairGlyph;
		}

		public void ApplySettings()
		{
			PlayerSettings.Fov = FovSlider.CalcValue;
			PlayerSettings.ViewmodelOffset = ViewmodelSlider.CalcValue;
			PlayerSettings.ViewmodelVisible = ViewmodelToggle.Value; 
			PlayerSettings.CrosshairVisible = CrosshairToggle.Value; 
			PlayerSettings.ViewmodelFlip = ViewmodelFlip.Value; 
			PlayerSettings.CrosshairGlyph = CrosshairGlyph.Text;

			PlayerSettings.Save();
		}

		private ViewModel GetViewModel()
		{
			var player = Local.Pawn as InstagibPlayer;
			var weapon = player.Inventory.Active as Railgun;
			
			return weapon.ViewModelEntity as ViewModel;
		}

		public void Toggle()
		{
			InstagibHud.ToggleMainMenu();
		}
	}
}
