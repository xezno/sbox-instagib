namespace Instagib;

[Library( "gib_crosshair_triangle" )]
public class TriangleCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack )
	{
		var center = Screen.Size / 2.0f;

		//
		// Properties
		//
		float size = 16f;
		float thickness = 1f;
		float gap = 8f;

		var color = Color.White;

		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		gap *= 2.0f.LerpTo( 1.0f, t );

		// top left
		GraphicsX.Line( color, thickness, center - new Vector2( gap + size, gap + size ), center - new Vector2( gap, gap ) );
		GraphicsX.Line( color, thickness, center - new Vector2( -gap - size, gap + size ), center - new Vector2( -gap, gap ) );

		// S
		gap *= 1.5f;
		GraphicsX.Line( color, thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		// dot
		GraphicsX.Circle( center, color, 1f );
	}
}
