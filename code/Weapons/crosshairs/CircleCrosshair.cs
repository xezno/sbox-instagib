namespace Instagib;

[Library( "gib_crosshair_circle" )]
public class CircleCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack )
	{
		var center = Screen.Size / 2.0f;

		//
		// Properties
		//
		float radius = 16f;
		int count = 3;
		float gap = 20;

		//
		// Animation / easing
		//
		float t = timeSinceAttack.Relative.LerpInverse( 0, 1.5f );
		t = Easing.EaseOut( t );

		var color = Color.White.WithAlpha( t );
		radius *= 2.0f.LerpTo( 1.0f, t );
		gap *= 2.0f.LerpTo( 0.0f, t );

		//
		// Circle crosshair
		//
		float interval = 360 / count;
		for ( int i = 0; i < count; ++i )
		{
			float startAngle = gap + (interval * i);
			float endAngle = (interval * (i + 1)) - gap;

			GraphicsX.Circle( center, radius, radius - 1f, color, startAngle: startAngle, endAngle: endAngle );
		}

		// Dot
		GraphicsX.Circle( center, color, 1f );
	}
}
