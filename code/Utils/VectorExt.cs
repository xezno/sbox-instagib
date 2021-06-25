﻿namespace Instagib.Utils
{
	public static class VectorExt
	{
		public static bool Inside( this Vector2 a, Vector2 b )
		{
			return (a.x > b.x - 1) || (a.y > b.y - 1);
		}

		public static bool Outside( this Vector2 a, Vector2 b )
		{
			return (a.x < b.x) || (a.y < b.y);
		}
	}
}
