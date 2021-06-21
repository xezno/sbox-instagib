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
		private (float, float) fovRange = ( 70f, 130f );
		private (float, float) viewmodelRange = ( 0f, 10f );
		
		public MainMenu()
		{
			SetTemplate( "/Code/UI/Menus/MainMenu.html" );
			StyleSheet.Load( "/Code/UI/Menus/MainMenu.scss" ); // Loading in HTML doesn't work for whatever reason

			FovSlider.OnValueChange += value =>
			{
				if ( Local.Pawn is InstagibPlayer player )
				{
					if ( !player.IsValid() || player.Health <= 0 )
						return;

					if ( player.Camera is FirstPersonCamera camera )
					{
						camera.defaultFov = value;
					}
				}
			};
			FovSlider.SnapRate = 5;
			FovSlider.ValueCalcFunc = value => fovRange.Item1.LerpTo( fovRange.Item2, value ).CeilToInt();

			FovSlider.Value = 110f;

			ViewmodelSlider.OnValueChange += v => ViewModel.Offset = v;
			ViewmodelSlider.SnapRate = 1;
			ViewmodelSlider.ValueCalcFunc = value => viewmodelRange.Item1.LerpTo( viewmodelRange.Item2, value ).CeilToInt();
			
			CrosshairToggle.Value = Crosshair.Visible;
			CrosshairToggle.OnValueChange += b => Crosshair.Visible = b;

			ViewmodelToggle.Value = ViewModel.Visible;
			ViewmodelToggle.OnValueChange += b => ViewModel.Visible = b;

			ViewmodelFlip.Value = ViewModel.Flip;
			ViewmodelFlip.OnValueChange += b => ViewModel.Flip = b;
			
			CrosshairGlyph.AddEvent("onchange", () =>
			{
				Crosshair.SetCrosshair( CrosshairGlyph.Text );
			});
			
			// Load settings
			FovSlider.Value = Cookie.Get<float>( "Instagib.Fov", 100 ).LerpInverse( fovRange.Item1, fovRange.Item2 );
			ViewmodelSlider.Value = Cookie.Get<float>( "Instagib.ViewmodelOffset", 0 ).LerpInverse( viewmodelRange.Item1, viewmodelRange.Item2 );
			ViewmodelToggle.Value = Cookie.Get( "Instagib.ViewmodelVisible", true );
			CrosshairToggle.Value = Cookie.Get( "Instagib.CrosshairVisible", true );
			ViewmodelFlip.Value = Cookie.Get( "Instagib.ViewmodelFlip", false );
			CrosshairGlyph.Text = Cookie.Get( "Instagib.CrosshairGlyph", "t" );
		}

		public override void OnDeleted()
		{
			// Save settings
			Cookie.Set( "Instagib.Fov", FovSlider.CalcValue );
			Cookie.Set( "Instagib.ViewmodelOffset", ViewmodelSlider.CalcValue );
			Cookie.Set( "Instagib.ViewmodelVisible", ViewmodelToggle.Value );
			Cookie.Set( "Instagib.CrosshairVisible", CrosshairToggle.Value );
			Cookie.Set( "Instagib.ViewmodelFlip", ViewmodelFlip.Value );
			Cookie.Set( "Instagib.CrosshairGlyph", CrosshairGlyph.Text );
			base.OnDeleted();	
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
