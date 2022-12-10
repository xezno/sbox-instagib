namespace Instagib;

[Library( "gib_crosshair_cross" )]
public class CrossCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack )
	{
		var center = Screen.Size / 2.0f;

		//
		// Properties
		//
		float size = 8f;
		float thickness = 1f;
		float gap = 8f;

		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		gap *= 2.0f.LerpTo( 1.0f, t );

		// N
		GraphicsX.Line( Color.White, thickness, center - new Vector2( 0, gap + size ), center - new Vector2( 0, gap ) );

		// S
		GraphicsX.Line( Color.White, thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		// E
		GraphicsX.Line( Color.White, thickness, center - new Vector2( gap + size, 0 ), center - new Vector2( gap, 0 ) );

		// W
		GraphicsX.Line( Color.White, thickness, center + new Vector2( gap + size, 0 ), center + new Vector2( gap, 0 ) );
	}
}
