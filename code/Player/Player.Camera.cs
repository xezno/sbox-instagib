﻿namespace Instagib;

partial class Player
{
	private float FovScale { get; set; }
	private float Fov { get; set; }
	private float WalkBob { get; set; }

	private float ZoomSpeed => 25.0f;

	// TODO: conditions for zooming
	public bool IsZooming => Input.Down( InputButton.SecondaryAttack );

	private void SimulateCamera()
	{
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.Position = EyePosition;
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 1f;
		Camera.ZFar = 5000.0f;

		var defaultFieldOfView = Screen.CreateVerticalFieldOfView( Local.UserPreference.FieldOfView );
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
