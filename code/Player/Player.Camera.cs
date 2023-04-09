namespace Instagib;

partial class Player
{
	private float FovScale { get; set; }
	private float Fov { get; set; }
	private float WalkBob { get; set; }

	private float ZoomSpeed => 25.0f;

	// TODO: conditions for zooming
	public bool IsZooming => Input.Down( InputButton.Zoom );

	private void SimulateCamera()
	{
		if ( LifeState != LifeState.Alive )
		{
			var direction = (Position - Camera.Position).Normal;
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, Rotation.LookAt( direction ), Time.Delta );

			return;
		}

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.Position = EyePosition;
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 1f;
		Camera.ZFar = 5000.0f;

		var defaultFieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		FovScale = 1.0f;

		//
		// Camera zoom
		//
		if ( IsZooming )
			FovScale = 0.5f;

		//
		// Dash zoom
		//
		if ( Controller.IsDashing )
			FovScale = 1.1f;

		//
		// Apply desired FOV over time
		//
		Fov = Fov.LerpTo( FovScale * defaultFieldOfView, ZoomSpeed * Time.Delta );
		Camera.FieldOfView = Fov;

		//
		// View bobbing
		//
		var speed = Velocity.Length;
		float t = speed.LerpInverse( 0, 310 );

		if ( GroundEntity != null )
			WalkBob += Time.Delta * 20.0f * t;

		var offset = Bobbing.CalculateOffset( WalkBob, t, 2.0f ) * Camera.Rotation;
		Camera.Position += offset;
	}
}
