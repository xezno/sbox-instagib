namespace Instagib;

public class ViewModel : BaseViewModel
{
	[ConVar.Replicated( "gib_debug_viewmodel" )] public static bool Debug { get; set; }

	private const float DefaultFov = 50f;

	private float Fov = DefaultFov;
	private Vector3 Offset;

	private float TargetFov = DefaultFov;
	private Vector3 TargetOffset;

	private Rotation TargetRotation;

	private float WalkBob;
	private float OffsetLerpRate = 10f;
	private float FovLerpRate = 25f;

	private float Kickback = 0f;

	private Vector3 ViewmodelPosition => new Vector3( 8, -8, -10 );

	// ============================================================

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();

		// Shit way of hiding viewmodel
		if ( Owner.LifeState != LifeState.Alive )
		{
			Position = new Vector3( 1000000 );
			return;
		}

		TargetRotation = Rotation.Lerp( TargetRotation, Camera.Rotation, 33f * Time.Delta );
		Rotation = Rotation.Lerp( Camera.Rotation, TargetRotation, 0.1f );

		TargetFov = DefaultFov;

		BuildWalkEffects();
		ApplyEffects();

		if ( Debug )
		{
			DebugOverlay.ScreenText( "[VIEWMODEL]\n" +
				$"TargetOffset:                {TargetOffset}\n" +
				$"Position:                    {Position}\n" +
				$"Fov:                         {Fov}\n" +
				$"Rotation:                    {Rotation}",
				new Vector2( 60, 250 ) );
		}
	}

	private void ApplyEffects()
	{
		Fov = Fov.LerpTo( TargetFov, FovLerpRate * Time.Delta );

		Offset = Offset.LerpTo( TargetOffset, OffsetLerpRate * Time.Delta );
		Position += -Offset * Rotation;
		Position += ViewmodelPosition * Rotation;
		Position += Rotation.Backward * Kickback;

		Kickback = Kickback.LerpTo( 0.0f, Time.Delta * 10f );

		Camera.Main.SetViewModelCamera( Fov );
	}

	public void OnFire()
	{
		Kickback += 32f;
	}

	private void BuildWalkEffects()
	{
		if ( Owner is Player player )
		{
			var speed = player.Velocity.Length;
			float t = speed.LerpInverse( 0, 310 );

			if ( Owner.GroundEntity != null )
				WalkBob += Time.Delta * 20.0f * t;

			float factor = 2.0f;
			TargetOffset = Bobbing.CalculateOffset( WalkBob, t, factor ) * Camera.Rotation;
			TargetOffset += new Vector3( t, 0, t / 2f ) * factor;

			if ( player.IsZooming )
				TargetFov = 30f;
		}
	}
}
