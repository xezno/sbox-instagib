namespace Instagib;

[Library( "gib_crosshair_dot" )]
public class DotCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack )
	{
		var center = Screen.Size / 2.0f;

		//
		// Properties
		//

		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		var color = Color.White.WithAlpha( t );

		// Dot
		GraphicsX.Circle( center, color, 1f );
	}
}
