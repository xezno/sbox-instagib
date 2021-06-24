using System;
using Sandbox;

namespace Instagib.Utils
{
	public static class ColorUtils
	{
		public struct HSVColor
		{
			public float hue;
			public float saturation;
			public float value;
			
			public HSVColor( float hue, float saturation, float value )
			{
				this.hue = hue;
				this.saturation = saturation;
				this.value = value;
			}
		}
		
		public static Color HSVToColor( this HSVColor hsv )
		{
			var hi = Convert.ToInt32( MathF.Floor( hsv.hue / 60.0f ) ) % 6;
			var f = hsv.hue / 60.0f - MathF.Floor( hsv.hue / 60.0f );

			var v = hsv.value;
			var p = hsv.value * (1 - hsv.saturation);
			var q = hsv.value * (1 - f * hsv.saturation);
			var t = hsv.value * (1 - (1 - f) * hsv.saturation);

			return hi switch
			{
				0 => new Color( v, t, p ),
				1 => new Color( q, v, p ),
				2 => new Color( p, v, t ),
				3 => new Color( p, q, v ),
				4 => new Color( t, p, v ),
				_ => new Color( v, p, q )
			};
		}

		private static float CalcHue( this Color col )
		{
			float min = Math.Min( Math.Min( col.r, col.g ), col.b );
			float max = Math.Max( Math.Max( col.r, col.g ), col.b );

			if ( min.AlmostEqual( max ) ) return 0;

			float hue;
			if ( max.AlmostEqual( col.r ) )
				hue = (col.g - col.b) / (max - min);
			else if ( max.AlmostEqual( col.g ) )
				hue = 2f + (col.b - col.r) / (max - min);
			else
				hue = 4f + (col.r - col.g) / (max - min);

			hue *= 60;
			if ( hue < 0 ) hue += 360;

			return MathF.Round( hue );
		}
		
		public static HSVColor ColorToHSV( this Color color )
		{
			float max = MathF.Max( color.r, Math.Max( color.g, color.b ) );
			float min = MathF.Min( color.r, Math.Min( color.g, color.b ) );

			float hue = color.CalcHue();
			float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
			float value = max / 255f;

			return new HSVColor( hue, saturation, value );
		}
		
		public static byte ComponentToByte( float v ) => (byte)MathF.Floor( (v >= 1.0f) ? 255f : v * 256.0f );
	}
}
