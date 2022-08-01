namespace Instagib;

partial class Player
{
	private float FovScale { get; set; }
	private float Fov { get; set; }
	private float WalkBob { get; set; }

	private float ZoomSpeed => 25.0f;

	// TODO: conditions for zooming
	public bool IsZooming => Input.Down( InputButton.SecondaryAttack );

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		var defaultFieldOfView = setup.FieldOfView;
		FovScale = 1.0f;

		//
		// Camera zoom
		//
		if ( IsZooming )
			FovScale = 0.75f;

		//
		// Dash zoom
		//
		if ( Controller.IsDashing )
			FovScale = 1.1f;

		//
		// Apply desired FOV over time
		//
		Fov = Fov.LerpTo( FovScale * defaultFieldOfView, ZoomSpeed * Time.Delta );
		setup.FieldOfView = Fov;

		//
		// Fire PostCameraSetup on active weapon so that it can do any custom stuff too
		//
		if ( ActiveChild != null )
			ActiveChild.PostCameraSetup( ref setup );

		//
		// View bobbing
		//
		var speed = Velocity.Length;
		float t = speed.LerpInverse( 0, 310 );

		if ( GroundEntity != null )
			WalkBob += Time.Delta * 20.0f * t;

		var offset = Bobbing.CalculateOffset( WalkBob, t, 2.0f ) * setup.Rotation;
		setup.Position += offset;
	}
}
