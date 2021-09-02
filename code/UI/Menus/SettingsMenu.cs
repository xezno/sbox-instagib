using System;
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
		public Slider CrosshairSlider { get; set; }
		public ColorPicker EnemyOutlineColor { get; set; }

		public Panel Scroll { get; set; }

		struct Range
		{
			public float Min { get; set; }
			public float Max { get; set; }

			public Range( float min, float max )
			{
				Min = min;
				Max = max;
			}

			public static implicit operator Range( ValueTuple<float, float> a )
			{
				return new( a.Item1, a.Item2 );
			}
		}

		private Range fovRange = (70f, 130f);
		private Range viewmodelRange = (0f, 10f);
		private Range crosshairRange = (16f, 64f);

		public SettingsMenu()
		{
			SetTemplate( "/Code/UI/Menus/SettingsMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/SettingsMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => fovRange.Min.LerpTo( fovRange.Max, value ).CeilToInt();
			FovSlider.Value = 110f;

			ViewmodelSlider.SnapRate = 1;
			ViewmodelSlider.ValueCalcFunc =
				value => viewmodelRange.Min.LerpTo( viewmodelRange.Max, value ).CeilToInt();
			
			CrosshairSlider.SnapRate = 2;
			CrosshairSlider.ValueCalcFunc =
				value => crosshairRange.Min.LerpTo( crosshairRange.Max, value ).CeilToInt();
			
			// Make it so that we can preview the settings live
			FovSlider.OnValueChange += v => PlayerSettings.Fov = v;
			ViewmodelSlider.OnValueChange += v => PlayerSettings.ViewmodelOffset = v;
			
			CrosshairToggle.AddEventListener( "onchange", e => PlayerSettings.CrosshairVisible = (bool)e.Value );
			ViewmodelToggle.AddEventListener( "onchange", e => PlayerSettings.ViewmodelVisible = (bool)e.Value );
			ViewmodelFlip.AddEventListener( "onchange", e => PlayerSettings.ViewmodelFlip = (bool)e.Value );
			
			CrosshairSlider.OnValueChange += b => PlayerSettings.CrosshairSize = b;
			EnemyOutlineColor.OnValueChange += c => PlayerSettings.EnemyOutlineColor = c;

			// Set values to existing settings
			PlayerSettings.Load();
			
			FovSlider.Value = PlayerSettings.Fov.LerpInverse( fovRange.Min, fovRange.Max );
			ViewmodelSlider.Value = PlayerSettings.ViewmodelOffset.LerpInverse( viewmodelRange.Min, viewmodelRange.Max );
			ViewmodelToggle.Checked = PlayerSettings.ViewmodelVisible;
			CrosshairToggle.Checked = PlayerSettings.CrosshairVisible;
			ViewmodelFlip.Checked = PlayerSettings.ViewmodelFlip;
			CrosshairSlider.Value = ((float)PlayerSettings.CrosshairSize).LerpInverse( crosshairRange.Min, crosshairRange.Max );

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
	}
}
