namespace Instagib;

partial class Player
{
	[Net, Predicted] public bool IsGrappling { get; set; }
	[Net] public Vector3 GrappleTarget { get; set; }

	private Particles grappleParticles;

	private float PullStrength => 16f;
	private float BoostStrength => 4f;
	private float AntiVelocityScale => 1f;
	private float MaxDistance => 1500f;
	private float GrappleTraceRadius => 32f;

	private void ClearGroundEntity()
	{
		if ( Controller is QuakeWalkController controller )
		{
			GroundEntity = null;
			controller.GroundEntity = null;
		}
	}

	public void SimulateGrapple( Client cl )
	{
		if ( !InstagibGame.GrapplesEnabled )
			return;

		if ( Input.Pressed( InputButton.Use ) && !IsGrappling )
		{
			DeployGrapple();
		}
		else if ( !Input.Down( InputButton.Use ) && IsGrappling )
		{
			RemoveGrapple();
		}

		if ( IsGrappling )
		{
			ClearGroundEntity();

			Vector3 playerVel = Velocity;
			Vector3 playerLookDir = EyeRotation.Forward;
			Velocity += playerLookDir * BoostStrength;

			Vector3 playerLatchDir = (Position - GrappleTarget).Normal;
			Velocity -= playerLatchDir * PullStrength;

			float projLength = playerVel.Dot( playerLatchDir );
			if ( projLength > 0 )
			{
				Vector3 projVector = projLength * playerLatchDir;
				Velocity -= AntiVelocityScale * projVector;
			}
		}
	}

	private TraceResult GrappleTrace( out Vector3 calcEndPos )
	{
		var startPos = EyePosition;
		var endPos = startPos + EyeRotation.Forward * MaxDistance;

		var tr = Trace.Ray( startPos, endPos )
			.Ignore( this )
			.WorldAndEntities()
			.WithoutTags( "player" )
			.Run();

		calcEndPos = tr.EndPosition;

		return tr;
	}

	protected virtual void DeployGrapple()
	{
		var tr = GrappleTrace( out var calcEndPos );

		// Grapple missed
		if ( !tr.Hit || !tr.Entity.IsValid() )
		{
			using ( Prediction.Off() )
				PlaySound( "player_use_fail" );
			return;
		}

		IsGrappling = true;

		using ( Prediction.Off() )
		{
			ClearGroundEntity();

			if ( IsServer )
			{
				GrappleTarget = calcEndPos;

				var rope = Particles.Create( "particles/grapple_beam.vpcf" );
				rope.SetOrientation( 0, Rotation.LookAt( GrappleTarget - this.Position ).Normal );
				rope.SetPosition( 0, GrappleTarget );
				rope.SetEntity( 1, this, Vector3.Up * 32 );

				grappleParticles = rope;
			}
		}
	}

	private void RemoveGrapple()
	{
		IsGrappling = false;
		DeleteParticles();
	}


	private void DeleteParticles()
	{
		if ( IsServer )
		{
			grappleParticles?.Destroy( true );
		}
	}
}
