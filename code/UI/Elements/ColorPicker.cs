using System;
using Instagib.Utils;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class ColorPicker : Panel
	{
		private class HexInput : Panel
		{
			private TextEntry hexEntry;
			
			public HexInput()
			{
				var hexPanel = Add.Panel();
				hexPanel.Add.Label( "Hex" );
				hexEntry = hexPanel.Add.TextEntry( "0" );
			}

			public void SetValue( Color color )
			{
				hexEntry.Text = color.Hex;
			}
		}

		private class PickedColor : Panel { }

		private byte[] imageData;
		private int width = 360;
		private int height = 150;
		private int stride = 4;

		private Slider valueSlider;
		private HexInput input;
		private Image image;
		private PickedColor pickedColor;

		private bool move;
		
		public ColorPicker()
		{
			StyleSheet.Load( "/Code/UI/Elements/ColorPicker.scss" );
			
			image = Add.Image();

			CreateTexture();
			
			pickedColor = AddChild<PickedColor>();
				
			var valPanel = Add.Panel();
			// valPanel.Add.Label( "Value" );
			valueSlider = valPanel.AddChild<Slider>();
			valueSlider.SnapRate = 5;
			valueSlider.Value = 1.0f;
			valueSlider.OnValueChange += value =>
			{
				CreateTexture( value );
			};
		}

		private void CreateTexture( int value = 0 )
		{
			float fValue = value / 100f;
			var hslColor = Color.Red.ToHsv();

			hslColor.value = fValue;

			var data = new byte[width * height * stride];
			imageData = data;

			for ( int i = 0; i < data.Length; ++i )
				data[i] = 255;

			void SetPixel( int x, int y, Color col )
			{
				data[((x + (y * width)) * stride) + 0] = ColorUtils.ComponentToByte( col.r );
				data[((x + (y * width)) * stride) + 1] = ColorUtils.ComponentToByte( col.g );
				data[((x + (y * width)) * stride) + 2] = ColorUtils.ComponentToByte( col.b );
				data[((x + (y * width)) * stride) + 3] = 255;
			}

			for (int y = 0; y < height; y++)
			{
				hslColor.hue = 0;
				
				for (int x = 0; x < width; x++)
				{
					var hsvConvert = hslColor.ToColor();
					SetPixel( x, y, hsvConvert );
					hslColor.hue += 1;
				}
				hslColor.saturation -= (y * 0.0001f);
			}

			var texture = Texture.Create( width, height ).WithStaticUsage().WithData( data ).WithName( "hsvColor" ).Finish();

			image.Texture = texture;
		}

		private void MoveShit()
		{
			Color GetPixel( int x, int y )
			{
				var col = new Color
				{
					r = imageData[((x + (y * width)) * stride) + 0] / 255f,
					g = imageData[((x + (y * width)) * stride) + 1] / 255f,
					b = imageData[((x + (y * width)) * stride) + 2] / 255f,
					a = imageData[((x + (y * width)) * stride) + 3] / 255f
				};

				return col;
			}

			var localPos = Mouse.Position - image.Box.Rect.Position;
			if ( localPos.Outside( Vector2.Zero ) || localPos.Inside( image.Box.Rect.Size ) )
				return;
			
			var normalizedPos = localPos / image.Box.Rect.Size;

			var arrayPos = normalizedPos * new Vector2( width, height );
			var arrayEntry = GetPixel( (int)arrayPos.x, (int)arrayPos.y );
			
			pickedColor.Style.Left = MousePosition.x;
			pickedColor.Style.Top = MousePosition.y;
			pickedColor.Style.Opacity = 1;
			pickedColor.Style.Dirty();
			pickedColor.Style.BackgroundColor = arrayEntry;
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

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );
			MoveShit();
		}

		public override void Tick()
		{
			if ( move )
				MoveShit();
		}
	}
}
