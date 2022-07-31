namespace Instagib;

//
// This isn't exactly like the Quake movement controller
// ( some quirks have been tweaked or removed ) but it's much
// closer to arena shooter movement than the default WalkController
//
//
// https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c
//
public partial class QuakeWalkController : BasePlayerController
{
	private Vector3 mins;
	private Vector3 maxs;

	[Net, Predicted] private Vector3 Impulse { get; set; }

	private bool Walking { get; set; }
	private bool GroundPlane { get; set; }
	private TraceResult GroundTrace { get; set; }
	private Unstuck Unstuck { get; set; }
	private Duck Duck { get; set; }
	private bool CanDoubleJump { get; set; }

	public QuakeWalkController()
	{
		Duck = new( this );
		Unstuck = new Unstuck( this );
		CanDoubleJump = true;
	}

	public void ApplyImpulse( Vector3 impulse )
	{
		Impulse += impulse;
	}

	public override void FrameSimulate()
	{
		EyeRotation = Input.Rotation;
	}

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	public virtual void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;

		var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

		Duck.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

		SetBBox( mins, maxs );
	}

	private void UpdateVelocity()
	{
		if ( Impulse.Length > 0 )
		{
			// HACK: Cancel out Z velocity so that gravity doesn't affect any impulse added
			// Velocity = Velocity.WithZ( 0 );

			// HACK:
			// Apply impulse directly (rather than additively) so that we go in the direction
			// that the impulse wanted (prevents players doing stupid shit like launching
			// themselves off the map because they weren't paying enough attention)
			Velocity = Impulse;
			Impulse = Vector3.Zero;
			SetGroundEntity( null );
		}
	}

	public override void Simulate()
	{
		EyeRotation = Input.Rotation;
		EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);

		if ( SpeedLimit > 0f )
		{
			if ( Velocity.Length > SpeedLimit )
			{
				Velocity = Velocity.Normal * SpeedLimit;
			}
		}

		// debug line reset
		line = 0;

		// ducking
		Duck.PreTick();

		// update bounding box post-duck
		UpdateBBox();

		// predicted trigger update
		UpdatePredictedTriggers();

		// update velocity based on any impulse applied
		UpdateVelocity();

		// set groundentity
		TraceToGround();

		// dashing
		if ( !ApplyDash() )
		{
			if ( CheckJump() || GroundEntity == null )
			{
				// gravity start
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

				// jumped away or in air
				AirMove();
			}
			else
			{
				// walking on ground
				WalkMove();
			}
		}

		// stick to ground
		CategorizePosition();

		// set groundentity
		TraceToGround();

		// check if we're stuck and fix that
		Unstuck.TestAndFix();

		// stay on ground
		if ( Debug && Pawn.IsLocalPawn )
		{
			DebugOverlay.ScreenText( $"[QUAKE WALK CONTROLLER]\n" +
				$"Velocity:                    {Velocity}\n" +
				$"Vel length:                  {Velocity.Length}\n" +
				$"Position:                    {Position}\n" +
				$"GroundEntity:                {GroundEntity}\n" +
				$"CanDoubleJump:               {CanDoubleJump}\n" +
				$"TouchingTriggers:            {string.Join( ',', TouchingTriggers.Select( x => x.Name ) )}",
				new Vector2( 360, 150 ), Time.Delta * 2.0f );

			DebugOverlay.Box( Position, mins, maxs, Color.Red );
		}
	}

	private Vector3 ClipVelocity( Vector3 inVec, Vector3 normal, float overbounce )
	{
		float backoff = inVec.Dot( normal );

		if ( backoff < 0 )
			backoff *= overbounce;
		else
			backoff /= overbounce;

		Vector3 outVec = inVec - (normal * backoff);
		return outVec;
	}

	private void ApplyFriction()
	{
		if ( GroundEntity == null )
			return;

		Vector3 vec = Velocity;

		if ( Walking )
			vec.z = 0;

		float speed = vec.Length;
		float control;

		if ( speed < 1 )
		{
			Vector3 vel = new( Velocity );
			vel.x = 0;
			vel.y = 0;
			Velocity = vel;
			return;
		}

		float drop = 0;

		if ( Walking )
		{
			control = speed < StopSpeed ? StopSpeed : speed;
			drop += control * Friction * Time.Delta;
		}

		float newspeed = speed - drop;

		if ( newspeed < 0 )
			newspeed = 0;

		newspeed /= speed;

		Velocity *= newspeed;
	}

	private void Accelerate( Vector3 wishDir, float wishSpeed, float maxSpeed, float accel )
	{
		if ( maxSpeed > 0 && wishSpeed > maxSpeed )
			wishSpeed = maxSpeed;

		switch ( AccelMode )
		{
			case AccelModes.Quake2:
				// Quake 2 style, allows for strafe jumps
				{

					float currentspeed = Velocity.Dot( wishDir );
					float addspeed = wishSpeed - currentspeed;

					if ( addspeed <= 0 )
						return;

					float accelspeed = accel * Time.Delta * wishSpeed;
					accelspeed = MathF.Min( accelspeed, addspeed );

					Velocity += accelspeed * wishDir;
				}
				break;
			case AccelModes.NoStrafeJump:
				// Avoids strafe jump maxspeed bug, but feels bad
				{
					// TODO: This does not preserve impulse velocity and so
					// anything that uses that will break while using this accel mode
					Vector3 wishVelocity = wishDir * wishSpeed;

					// We do not want to cancel out gravity, so we strip
					// the Z component here
					Vector3 pushDir = wishVelocity - Velocity.WithZ( 0 );

					float pushLen = pushDir.Length;
					float canPush = accel * Time.Delta * wishSpeed;

					canPush = MathF.Min( canPush, pushLen );

					Velocity += canPush * pushDir.Normal;
				}
				break;
		}
	}

	private bool IsDashing { get; set; }
	private Vector3 DashStart { get; set; }
	private Vector3 DashEnd { get; set; }
	private float DashProgress { get; set; }

	private Vector3 GetDashDirection()
	{
		var dir = GetWishDirection();

		if ( dir.Length.AlmostEqual( 0 ) )
		{
			dir = Pawn.EyeRotation.Forward;
		}

		dir = dir.WithZ( 0 ).Normal;

		return dir;
	}

	private bool ApplyDash()
	{
		// AG: this probably isn't a great way to do dashing
		// (we're just setting the position manually) but
		// it's consistent enough to be used here

		if ( Input.Pressed( InputButton.Run ) )
		{
			var dir = GetDashDirection();
			var startPos = Position;
			var endPos = startPos + dir * DashDistance;

			var tr = TraceBBox( startPos, endPos );

			DashStart = tr.StartPosition;
			DashEnd = tr.EndPosition;

			DebugOverlay.Sphere( DashStart, 4f, Color.Green, 5f, false );
			DebugOverlay.Sphere( DashEnd, 4f, Color.Red, 5f, false );

			DashProgress = 0f;

			IsDashing = true;
		}

		if ( IsDashing )
		{
			DashProgress += Time.Delta * 10f;
			Position = DashStart.LerpTo( DashEnd, DashProgress );

			if ( DashProgress >= 1.0f )
			{
				IsDashing = false;
				Velocity = (DashEnd - DashStart).Normal * Velocity.Length;
			}

			return true;
		}

		return false;
	}

	private bool CheckJump()
	{
		if ( GroundEntity == null && !CanDoubleJump )
			return false;

		float jumpVel = JumpVelocity;

		if ( GroundEntity != null )
		{
			if ( !Input.Down( InputButton.Jump ) )
				return false;
		}
		else if ( CanDoubleJump )
		{
			if ( !Input.Pressed( InputButton.Jump ) )
				return false;

			jumpVel *= 1.25f;
			CanDoubleJump = false;

			var boostParticles = Particles.Create( "particles/boost.vpcf", this.Position );
			boostParticles.SetForward( 0, Vector3.Up );
		}

		if ( Duck.IsActive )
		{
			jumpVel *= 0.75f;
		}

		Velocity = Velocity.WithZ( jumpVel );

		SetGroundEntity( null );

		return true;
	}

	private void AirMove()
	{
		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, AirControl, AirAcceleration );

		if ( GroundPlane )
		{
			Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
		}

		StepSlideMove( true );
	}

	private Vector3 GetWishDirection()
	{
		float fMove = Input.Forward;
		float sMove = Input.Left;

		Vector3 forward = EyeRotation.Forward.WithZ( 0 );
		Vector3 left = EyeRotation.Left.WithZ( 0 );

		forward = ClipVelocity( forward, GroundTrace.Normal, Overclip );
		left = ClipVelocity( left, GroundTrace.Normal, Overclip );

		forward = forward.Normal;
		left = left.Normal;

		Vector3 wishVel = fMove * forward + sMove * left;
		wishVel.z = 0;

		return wishVel.Normal;
	}

	private float GetWishSpeed()
	{
		float duckSpeed = Duck.GetWishSpeed();

		if ( duckSpeed > 0 )
			return duckSpeed;

		if ( GroundEntity == null )
			return AirSpeed;

		return Speed;
	}

	private void WalkMove()
	{
		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, 0, Acceleration );

		// Slide along the ground plane
		float vel = Velocity.Length;
		Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );

		// Don't decreate velocity when going up or down a slope
		Velocity = Velocity.Normal;
		Velocity *= vel;

		// Don't do anything if standing still
		if ( Velocity.Length.AlmostEqual( 0 ) )
		{
			return;
		}

		StepSlideMove( false );
	}

	public override TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid" )
					.Ignore( Pawn )
					.WithoutTags( "player" )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0 )
	{
		return TraceBBox( start, end, mins, maxs, liftFeet );
	}

	private bool CorrectAllSolid()
	{
		Vector3 point;

		LogToScreen( "CorrectAllSolid" );

		{
			for ( int i = -1; i <= 1; i++ )
			{
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int k = -1; k <= 1; k++ )
					{
						point = Position;
						point.x += i;
						point.y += j;
						point.z += k - 0.25f;

						var trace = TraceBBox( point, point );
						DebugOverlay.Line( Position, point );

						if ( !trace.StartedSolid )
						{
							LogToScreen( "Found space for correctallsolid" );
							DebugOverlay.Sphere( point, 2f, Color.White );

							point = Position.WithZ( Position.z - GroundDistance );
							trace = TraceBBox( Position, point );
							Position = point;
							GroundTrace = trace;

							return true;
						}
					}
				}
			}
		}

		SetGroundEntity( null );
		return false;
	}

	private void TraceToGround()
	{
		Vector3 point = new Vector3( Position ).WithZ( Position.z - GroundDistance );
		TraceResult trace = TraceBBox( Position, point );
		GroundTrace = trace;

		// do something corrective if the trace starts in a solid...
		if ( trace.StartedSolid )
		{
			LogToScreen( "do something corrective if the trace starts in a solid..." );
			// AG: we don't actually do anything here because
			// unstuck.testandfix() will handle it later on

			// uncomment for some scuffed 1990s stuck-handling code
			//if ( CorrectAllSolid() )
			//	return;
		}

		// if the trace didn't hit anything, we are in free fall
		if ( trace.Fraction == 1.0f )
		{
			SetGroundEntity( null );
		}

		// check if getting thrown off the ground
		if ( Velocity.z > 0 && Velocity.Dot( trace.Normal ) > 10.0f )
		{
			LogToScreen( $"Kickoff" );
			SetGroundEntity( null );
			return;
		}

		// slopes that are too steep will not be considered onground
		if ( trace.Entity != null && Vector3.GetAngle( Vector3.Up, trace.Normal ) > MaxWalkAngle )
		{
			LogToScreen( $"Too steep" );
			SetGroundEntity( null );
			return;
		}

		SetGroundEntity( trace );
	}

	private void CategorizePosition()
	{
		// if the player hull point one unit down is solid, the player is on ground
		// see if standing on something solid
		var point = Position + Vector3.Down * 1;
		var bumpPos = Position;

		bool moveToEndPos = false;

		if ( GroundEntity != null )
		{
			moveToEndPos = true;
			point.z -= StepSize;
		}

		var trace = TraceBBox( bumpPos, point );

		if ( trace.Entity == null || Vector3.GetAngle( Vector3.Up, trace.Normal ) > MaxWalkAngle )
		{
			SetGroundEntity( null );
			moveToEndPos = false;
		}
		else
		{
			SetGroundEntity( trace );
		}

		if ( moveToEndPos && !trace.StartedSolid && trace.Fraction > 0.0f && trace.Fraction < 1.0f )
		{
			Position = trace.EndPosition;
		}
	}

	private void SetGroundEntity( TraceResult tr )
	{
		SetGroundEntity( tr.Entity );
	}

	private void SetGroundEntity( Entity ent )
	{
		if ( ent == null )
		{
			GroundPlane = false;
			Walking = false;
		}
		else
		{
			GroundPlane = true;
			Walking = true;
			CanDoubleJump = true;
		}

		GroundEntity = ent;
	}
}
