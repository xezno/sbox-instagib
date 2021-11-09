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
		public Slider ViewTiltMultiplierSlider { get; set; }
		public ColorPicker CrosshairColor { get; set; }
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
		private Range viewTiltRange = (0f, 400f);

		public SettingsMenu()
		{
			SetTemplate( "/Code/UI/Menus/SettingsMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/SettingsMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => fovRange.Min.LerpTo( fovRange.Max, value ).CeilToInt();
			FovSlider.Value = 110f;

			ViewTiltMultiplierSlider.SnapRate = 1;
			ViewTiltMultiplierSlider.ValueCalcFunc =
				value => viewTiltRange.Min.LerpTo( viewTiltRange.Max, value ).CeilToInt();

			ViewmodelSlider.SnapRate = 1;
			ViewmodelSlider.ValueCalcFunc =
				value => viewmodelRange.Min.LerpTo( viewmodelRange.Max, value ).CeilToInt();
			
			// Make it so that we can preview the settings live
			FovSlider.OnValueChange += v => PlayerSettings.Fov = v;
			ViewmodelSlider.OnValueChange += v => PlayerSettings.ViewmodelOffset = v;
			ViewTiltMultiplierSlider.OnValueChange += v => PlayerSettings.ViewTiltMultiplier = v / 100f;

			ViewmodelToggle.AddEventListener( "onchange", e => PlayerSettings.ViewmodelVisible = (bool)e.Value );
			ViewmodelFlip.AddEventListener( "onchange", e => PlayerSettings.ViewmodelFlip = (bool)e.Value );

			CrosshairColor.OnValueChange += c => PlayerSettings.CrosshairColor = c;
			EnemyOutlineColor.OnValueChange += c => PlayerSettings.EnemyOutlineColor = c;

			// Set values to existing settings
			PlayerSettings.Load();
			
			FovSlider.Value = PlayerSettings.Fov.LerpInverse( fovRange.Min, fovRange.Max );
			ViewmodelSlider.Value = PlayerSettings.ViewmodelOffset.LerpInverse( viewmodelRange.Min, viewmodelRange.Max );

			ViewTiltMultiplierSlider.Value = (PlayerSettings.ViewTiltMultiplier * 100f) / viewTiltRange.Max;
			ViewmodelToggle.Checked = PlayerSettings.ViewmodelVisible;
			ViewmodelFlip.Checked = PlayerSettings.ViewmodelFlip;

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
