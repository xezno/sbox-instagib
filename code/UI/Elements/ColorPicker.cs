using System;
using Instagib.Utils;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class ColorPicker : Panel
	{
		private class RgbInput : Panel
		{
			private TextEntry rEntry;
			private TextEntry gEntry;
			private TextEntry bEntry;
			
			public RgbInput()
			{
				var rPanel = Add.Panel();
				rPanel.Add.Label( "R" );
				rEntry = rPanel.Add.TextEntry( "0" );
				
				var gPanel = Add.Panel();
				gPanel.Add.Label( "G" );
				gEntry = gPanel.Add.TextEntry( "0" );
				
				var bPanel = Add.Panel();
				bPanel.Add.Label( "B" );
				bEntry = bPanel.Add.TextEntry( "0" );
			}

			public void SetValues( Color color )
			{
				rEntry.Text = (color.r).ToString("F0");
				gEntry.Text = (color.g).ToString("F0");
				bEntry.Text = (color.b).ToString("F0");
			}
		}

		private byte[] imageData;
		private int width = 360;
		private int height = 150;
		private int stride = 4;

		private Slider valueSlider;
		private RgbInput input;
		private Image image;
		
		public ColorPicker()
		{
			StyleSheet.Load( "/Code/UI/Elements/ColorPicker.scss" );
			
			image = Add.Image();
			input = AddChild<RgbInput>();
			
			DoTextureStuff();
				
			var valPanel = Add.Panel();
			valPanel.Add.Label( "Value" );
			valueSlider = valPanel.AddChild<Slider>();
			valueSlider.SnapRate = 5;
			valueSlider.Value = 1.0f;
			valueSlider.OnValueChange += value =>
			{
				DoTextureStuff( value );
			};
		}

		private void DoTextureStuff( int value = 0 )
		{
			float fValue = value / 100f;
			
			var pixelColor = Color.Red;
			var hslColor = HSV.ColorToHSV( pixelColor );

			hslColor.value = fValue;

			var data = new byte[width * height * stride];
			imageData = data;

			for ( int i = 0; i < data.Length; ++i )
				data[i] = 255;

			void SetPixel( int x, int y, Color col )
			{ 
				byte ColToByte( float v ) => (byte)MathF.Floor( (v >= 1.0f) ? 255f : v * 256.0f );

				data[((x + (y * width)) * stride) + 0] = ColToByte( col.r );
				data[((x + (y * width)) * stride) + 1] = ColToByte( col.g );
				data[((x + (y * width)) * stride) + 2] = ColToByte( col.b );
				data[((x + (y * width)) * stride) + 3] = 255;
			}

			for (int y = 0; y < height; y++)
			{
				hslColor.hue = 0;
				
				for (int x = 0; x < width; x++)
				{
					var hsvConvert = hslColor.HSVToColor();
					pixelColor = hsvConvert;
					SetPixel( x, y, pixelColor );
					hslColor.hue += 1;
				}
				hslColor.saturation -= (y * 0.0001f); // 40 works, don't change it
			}

			var texture = Texture.Create( width, height ).WithStaticUsage().WithData( data ).WithName( "hsvColor" ).Finish();

			image.Texture = texture;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );
			
			Color GetPixel( int x, int y )
			{
				var col = new Color
				{
					r = imageData[((x + (y * width)) * stride) + 0],
					g = imageData[((x + (y * width)) * stride) + 1],
					b = imageData[((x + (y * width)) * stride) + 2],
					a = imageData[((x + (y * width)) * stride) + 3]
				};

				return col;
			}

			var localPos = Mouse.Position - image.Box.Rect.Position;
			if ( localPos.Outside( Vector2.Zero ) || localPos.Inside( image.Box.Rect.Size ) )
				return;
			
			var normalizedPos = localPos / image.Box.Rect.Size;

			var arrayPos = normalizedPos * new Vector2( width, height );
			
			var arrayEntry = GetPixel( (int)arrayPos.x, (int)arrayPos.y );

			input.SetValues( arrayEntry );
		}
	}
}
