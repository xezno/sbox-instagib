using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class Slider : Panel
	{
		private class SliderNeedle : Panel
		{
			private bool move;
			private Panel line;

			private Slider slider;
			
			public SliderNeedle( Panel line, Slider slider )
			{
				this.line = line;
				this.slider = slider;
				SetClass( "needle", true );
			}

			protected override void OnMouseDown( MousePanelEvent e )
			{
				base.OnMouseDown( e );
				move = true;
			}

			protected override void OnMouseUp( MousePanelEvent e )
			{
				base.OnMouseUp( e );
				move = false;
			}

			public void OneShot()
			{
				DoMove();
			}

			private void DoMove()
			{
				var leftPos = Mouse.Position.x - Parent.Box.Left;
				var width = line.Box.Rect.width;
				
				// I have zero idea why this works, but these numbers fix the offset bug.
				width *= 1.055f;
				leftPos *= 1.055f;
				// I should probably look into why - it's probably something to do with padding/margins.
					
				leftPos = leftPos.Clamp( 0, width );

				slider.Value = (leftPos / width);
				
				Style.Left = leftPos;
				Style.Dirty();
			}

			public void SetValue( float value )
			{
				var width = line.Box.Rect.width;
				var leftPos = (value * width);
				
				// Crappy offset bug fix again
				width *= 1.055f;
				leftPos *= 1.055f;
				
				Style.Left = leftPos;
				Style.Dirty();
			}

			public override void Tick()
			{
				base.Tick();
				if ( move )
				{
					DoMove();
				}
			}
		}
		
		private int lastValue = 0; // Used internally to detect value changes
		
		//
		// Elements
		//
		private Label text;
		private SliderNeedle needle;

		public int SnapRate { get; set; } = 1;
		
		//
		// API
		//

		private float value;

		/// <summary>
		/// The current slider value from 0 to 1.
		/// </summary>
		public new float Value
		{
			get => this.value;
			set
			{
				this.value = value;
				valueDirty = true;
			}
		}

		/// <summary>
		/// The user-facing value as calculated by <see cref="ValueCalcFunc"/> 
		/// </summary>
		public int CalcValue
		{
			get => ValueCalcFunc?.Invoke( Value ) ?? 0;
		} 

		/// <summary>
		/// This is where the value gets calculated. This is also shown to the user.
		/// </summary>
		public Func<float, int> ValueCalcFunc { get; set; } = val => (val * 100).CeilToInt();

		//
		// Events
		//
		public delegate void SliderChangeEvent( int newValue );
		
		/// <summary>
		/// Fired when the slider value changes.
		/// <para>
		/// The newValue parameter represents the same value as <see cref="Slider.CalcValue"/>.
		/// </para>
		/// </summary>
		public SliderChangeEvent OnValueChange;

		private bool valueDirty = false;

		public Slider()
		{
			StyleSheet.Load( "/Code/UI/Elements/Slider.scss" );

			var line = Add.Panel( "line" );
			
			text = Add.Label( "0" );

			needle = new SliderNeedle( line, this );
			needle.Parent = this;
		}

		public override void Tick()
		{
			base.Tick();
			
			var userValue = ValueCalcFunc?.Invoke( Value );

			userValue = (int)(Math.Round( (float)userValue / SnapRate ) * SnapRate);
			
			if ( lastValue != userValue )
			{
				PlaySound( "ui.button.over" );
				OnValueChange?.Invoke( userValue ?? 0 );
			}
			
			lastValue = userValue ?? 0;
			text.Text = userValue.ToString();
			
			if ( valueDirty )
			{
				needle.SetValue( Value );
				// TODO: Fix this crap!
				// valueDirty = false;
			}
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );
			needle.OneShot();
		}
	}
}
