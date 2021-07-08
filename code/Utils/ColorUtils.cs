using System;
using Sandbox;

namespace Instagib.Utils
{
	public static class ColorUtils
	{	
		public static byte ComponentToByte( float v ) => (byte)MathF.Floor( (v >= 1.0f) ? 255f : v * 256.0f );
	}
}
