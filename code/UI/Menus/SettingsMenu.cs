using Instagib.UI.Menus;
using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	public class SettingsMenu : BaseMenu
	{
		public Slider FovSlider { get; set; }
		public Slider ViewmodelSlider { get; set; }
		public Checkbox ViewmodelToggle { get; set; }
		public Checkbox ViewmodelFlip { get; set; }
		public Checkbox CrosshairToggle { get; set; }
		public TextEntry CrosshairGlyph { get; set; }
		public Slider CrosshairSlider { get; set; }
		public ColorPicker EnemyOutlineColor { get; set; }

		public Panel Scroll { get; set; }

		// TODO: Range struct
		private (float, float) fovRange = (70f, 130f);
		private (float, float) viewmodelRange = (0f, 10f);
		private (float, float) crosshairRange = (16f, 64f);

		public SettingsMenu()
		{
			SetTemplate( "/Code/UI/Menus/SettingsMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/SettingsMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => fovRange.Item1.LerpTo( fovRange.Item2, value ).CeilToInt();
			FovSlider.Value = 110f;

			ViewmodelSlider.SnapRate = 1;
			ViewmodelSlider.ValueCalcFunc =
				value => viewmodelRange.Item1.LerpTo( viewmodelRange.Item2, value ).CeilToInt();
			
			CrosshairSlider.SnapRate = 2;
			CrosshairSlider.ValueCalcFunc =
				value => crosshairRange.Item1.LerpTo( crosshairRange.Item2, value ).CeilToInt();
			
			// Make it so that we can preview the settings live
			FovSlider.OnValueChange += v => PlayerSettings.Fov = v;
			ViewmodelSlider.OnValueChange += v => PlayerSettings.ViewmodelOffset = v;
			CrosshairToggle.OnValueChange += b => PlayerSettings.CrosshairVisible = b;
			ViewmodelToggle.OnValueChange += b => PlayerSettings.ViewmodelVisible = b;
			ViewmodelFlip.OnValueChange += b => PlayerSettings.ViewmodelFlip = b;
			CrosshairSlider.OnValueChange += b => PlayerSettings.CrosshairSize = b;
			CrosshairGlyph.AddEvent("onchange", () => PlayerSettings.CrosshairGlyph = CrosshairGlyph.Text );
			EnemyOutlineColor.OnValueChange += c => PlayerSettings.EnemyOutlineColor = c;

			// Set values to existing settings
			PlayerSettings.Load();
			
			FovSlider.Value = PlayerSettings.Fov.LerpInverse( fovRange.Item1, fovRange.Item2 );
			ViewmodelSlider.Value = PlayerSettings.ViewmodelOffset.LerpInverse( viewmodelRange.Item1, viewmodelRange.Item2 );
			ViewmodelToggle.Value = PlayerSettings.ViewmodelVisible;
			CrosshairToggle.Value = PlayerSettings.CrosshairVisible;
			ViewmodelFlip.Value = PlayerSettings.ViewmodelFlip;
			CrosshairGlyph.Text = PlayerSettings.CrosshairGlyph;
			CrosshairSlider.Value = ((float)PlayerSettings.CrosshairSize).LerpInverse( crosshairRange.Item1, crosshairRange.Item2 );

			// Add scrollbar
			var scrollbar = AddChild<Scrollbar>();
			scrollbar.Panel = Scroll;
		}
		
		public void ApplySettings()
		{
			PlayerSettings.Save();
		}

		public void RestoreSettings()
		{
			PlayerSettings.Load();
		}

		private ViewModel GetViewModel()
		{
			var player = Local.Pawn as Player;
			var weapon = player.Inventory.Active as Railgun;
			
			return weapon.ViewModelEntity as ViewModel;
		}

		public void Toggle()
		{
			InstagibHud.CurrentHud.SetCurrentMenu( new MainMenu() );
		}
	}
}
