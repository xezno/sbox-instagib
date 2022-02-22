﻿using System;
using Sandbox;

namespace Instagib
{
	[Library]
	public partial class PlayerController : BasePlayerController
	{
		private bool IsTouchingLadder;
		private Vector3 LadderNormal;

		// Duck body height 32
		// Eye Height 64
		// Duck Eye Height 28
		protected Vector3 maxs;
		protected Vector3 mins;

		protected float SurfaceFriction;
		public Unstuck Unstuck;

		public PlayerController()
		{
			Unstuck = new Unstuck( this );
		}

		public float Speed => 310.0f;
		public float AirSpeed => 240.0f;
		public float Acceleration => 8.0f;
		public float AirAcceleration => 8.0f;
		public float GroundFriction => 4.0f;
		public float StopSpeed => 100.0f;
		public float DistEpsilon => 0.03125f;
		public float GroundAngle => 46.0f;
		public float StepSize => 18.0f;
		public float MaxNonJumpVelocity => 128.0f;
		public float BodyGirth => 32.0f;
		public float BodyHeight => 72.0f;
		public float EyeHeight => 64.0f;
		public float Gravity => 800.0f;
		public bool AutoJump => true;
		public float JumpMultiplier => 0.9f;
		public float AirSpeedLimit => 800f;
		public float SpeedLimit => 600f;

		[Net, Predicted] public bool CanMove { get; set; } = true;

		/// <summary>
		///     This is temporary, get the hull size for the player's collision
		/// </summary>
		public override BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );

			return new BBox( mins, maxs );
		}

		public virtual void SetBBox( Vector3 mins, Vector3 maxs )
		{
			if ( this.mins == mins && this.maxs == maxs )
			{
				return;
			}

			this.mins = mins;
			this.maxs = maxs;
		}

		/// <summary>
		///     Update the size of the bbox. We should really trigger some shit if this changes.
		/// </summary>
		public virtual void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

			SetBBox( mins, maxs );
		}


		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeRotation = Input.Rotation;
		}

		public override void Simulate()
		{
			EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyeLocalPosition += TraceOffset;
			EyeRotation = Input.Rotation;

			if ( Unstuck.TestAndFix() )
			{
				return;
			}

			CheckLadder();

			//
			// Start Gravity
			//
			if ( !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
			{
				CheckJumpButton();
			}

			// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor, 
			// we don't slow when standing still, relative to the conveyor.
			var startOnGround = GroundEntity != null;
			if ( startOnGround )
			{
				Velocity = Velocity.WithZ( 0 );

				if ( GroundEntity != null )
				{
					ApplyFriction( GroundFriction * SurfaceFriction );
				}
			}

			// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1

			WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
			WishVelocity *= Input.Rotation;

			if ( !IsTouchingLadder )
			{
				WishVelocity = WishVelocity.WithZ( 0 );
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();

			if ( !CanMove )
			{
				WishVelocity = Vector3.Zero;
			}

			var stayOnGround = false;

			if ( IsTouchingLadder )
			{
				LadderMove();
			}
			else if ( GroundEntity != null )
			{
				stayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();
			}

			CategorizePosition( stayOnGround );

			// FinishGravity
			if ( !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}

			LimitSpeed();

			if ( Debug )
			{
				DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
				DebugOverlay.Box( Position, mins, maxs, Color.Blue );

				var lineOffset = 0;
				if ( Host.IsServer )
				{
					lineOffset = 10;
				}

				DebugOverlay.ScreenText( lineOffset + 0, $"        Position: {Position}" );
				DebugOverlay.ScreenText( lineOffset + 1, $"        Velocity: {Velocity}" );
				DebugOverlay.ScreenText( lineOffset + 2, $"    BaseVelocity: {BaseVelocity}" );
				DebugOverlay.ScreenText( lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]" );
				DebugOverlay.ScreenText( lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}" );
				DebugOverlay.ScreenText( lineOffset + 5, $"    WishVelocity: {WishVelocity}" );
			}
		}

		public virtual void LimitSpeed()
		{
			float lerpRate = 5f;
			if ( GroundEntity != null )
			{
				if ( Velocity.WithZ( 0 ).Length > SpeedLimit )
				{
					Velocity = Velocity.LerpTo( (Velocity.Normal * AirSpeedLimit).WithZ( Velocity.z ), lerpRate * Time.Delta );
				}
			}
			else
			{
				if ( Velocity.WithZ( 0 ).Length > SpeedLimit )
				{
					Velocity = Velocity.LerpTo( (Velocity.Normal * SpeedLimit).WithZ( Velocity.z ), lerpRate * Time.Delta );
				}
			}
		}

		public virtual float GetWishSpeed()
		{
			if ( GroundEntity == null )
				return AirSpeed;

			return Speed;
		}

		public virtual void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishspeed;

			Velocity = Velocity.WithZ( 0 );
			Accelerate( wishdir, wishspeed, SpeedLimit, Acceleration );
			Velocity = Velocity.WithZ( 0 );

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination	
				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

				var pm = TraceBBox( Position, dest );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		public virtual void StepMove()
		{
			var startPos = Position;
			var startVel = Velocity;

			//
			// First try walking straight to where they want to go.
			//
			TryPlayerMove();

			//
			// mv now contains where they ended up if they tried to walk straight there.
			// Save those results for use later.
			//	
			var withoutStepPos = Position;
			var withoutStepVel = Velocity;

			//
			// Try again, this time step up and move across
			//
			Position = startPos;
			Velocity = startVel;
			var trace = TraceBBox( Position, Position + Vector3.Up * (StepSize + DistEpsilon) );
			if ( !trace.StartedSolid )
			{
				Position = trace.EndPosition;
			}

			TryPlayerMove();

			//
			// If we move down from here, did we land on ground?
			//
			trace = TraceBBox( Position, Position + Vector3.Down * (StepSize + DistEpsilon * 2) );
			if ( !trace.Hit || Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle )
			{
				// didn't step on ground, so just use the original attempt without stepping
				Position = withoutStepPos;
				Velocity = withoutStepVel;
				return;
			}

			if ( !trace.StartedSolid )
			{
				Position = trace.EndPosition;
			}

			var withStepPos = Position;

			var withoutStep = (withoutStepPos - startPos).WithZ( 0 ).Length;
			var withStep = (withStepPos - startPos).WithZ( 0 ).Length;

			//
			// We went further without the step, so lets use that
			//
			if ( withoutStep > withStep )
			{
				Position = withoutStepPos;
				Velocity = withoutStepVel;
			}
		}

		/// <summary>
		///     Add our wish direction and speed onto our velocity
		/// </summary>
		public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishspeed > speedLimit )
			{
				wishspeed = speedLimit;
			}

			// See if we are changing direction a bit
			var currentspeed = Velocity.Dot( wishdir );

			// Reduce wishspeed by the amount of veer.
			var addspeed = wishspeed - currentspeed;

			// If not going to add any speed, done.
			if ( addspeed <= 0 )
			{
				return;
			}

			// Determine amount of acceleration.
			var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

			// Cap at addspeed
			if ( accelspeed > addspeed )
			{
				accelspeed = addspeed;
			}

			Velocity += wishdir * accelspeed;
		}

		public virtual void AirAccelerate( Vector3 wishdir, float wishspeed, float acceleration )
		{
			// See if we are changing direction a bit
			var currentspeed = Velocity.Dot( wishdir );

			// Reduce wishspeed by the amount of veer.
			var addspeed = wishspeed - currentspeed;

			// If not going to add any speed, done.
			if ( addspeed <= 0 )
			{
				return;
			}

			// Determine amount of acceleration.
			var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

			// Cap at addspeed
			if ( accelspeed > addspeed )
			{
				accelspeed = addspeed;
			}

			Velocity += wishdir * accelspeed;
		}

		/// <summary>
		///     Remove ground friction from velocity
		/// </summary>
		public virtual void ApplyFriction( float frictionAmount = 1.0f )
		{
			// Calculate speed
			var speed = Velocity.Length;
			if ( speed < 0.1f )
			{
				return;
			}

			// Bleed off some speed, but if we have less than the bleed
			//  threshold, bleed the threshold amount.
			var control = speed < StopSpeed ? StopSpeed : speed;

			// Add the amount to the drop amount.
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			var newspeed = speed - drop;
			if ( newspeed < 0 )
			{
				newspeed = 0;
			}

			if ( newspeed != speed )
			{
				newspeed /= speed;
				Velocity *= newspeed;
			}

			// mv->m_outWishVel -= (1.f-newspeed) * mv->m_vecVelocity;
		}

		public virtual void CheckJumpButton()
		{
			if ( GroundEntity == null )
				return;

			if ( !CanMove )
				return;

			ClearGroundEntity();

			var flMul = 268.3281572999747f * (Gravity / 600f) * JumpMultiplier;
			var startz = Velocity.z;

			Velocity = Velocity.WithZ( startz + flMul );
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			Sound.FromWorld( "jump", Position );

			AddEvent( "jump" );
		}

		public virtual void AirMove()
		{
			var wishdir = (Input.Rotation.Forward * Input.Forward) + (Input.Rotation.Left * Input.Left);
			wishdir = Vector3.Lerp( wishdir, WishVelocity.Normal, 0.5f );
			wishdir = wishdir.WithZ( 0 );

			var wishspeed = WishVelocity.Length;

			AirAccelerate( wishdir, wishspeed, AirAcceleration );

			Velocity += BaseVelocity;

			TryPlayerMove();

			Velocity -= BaseVelocity;
		}

		public virtual void CheckLadder()
		{
			if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;
			}

			const float ladderDistance = 1.0f;
			var start = Position;
			var end = start + (IsTouchingLadder ? LadderNormal * -1.0f : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray( start, end )
				.Size( mins, maxs )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.LADDER )
				.Ignore( Pawn )
				.Run();

			IsTouchingLadder = false;

			if ( pm.Hit )
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		public virtual void LadderMove()
		{
			var velocity = WishVelocity;
			var normalDot = velocity.Dot( LadderNormal );
			var cross = LadderNormal * normalDot;
			Velocity = velocity - cross + -normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal );

			TryPlayerMove();
		}

		public virtual void TryPlayerMove()
		{
			var mover = new CustomMoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = GroundAngle;

			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public virtual void CategorizePosition( bool stayOnGround )
		{
			SurfaceFriction = 1.0f;

			// Doing this before we move may introduce a potential latency in water detection, but
			// doing it after can get us stuck on the bottom in water if the amount we move up
			// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
			// this several times per frame, so we really need to avoid sticking to the bottom of
			// water on each call, and the converse case will correct itself if called twice.
			//CheckWater();

			var point = Position - Vector3.Up * 2;
			var bumpOrigin = Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			var movingUpRapidly = Velocity.z > MaxNonJumpVelocity;
			var moveToEndPos = false;

			if ( GroundEntity != null ) // and not underwater
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( stayOnGround )
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}

			if ( movingUpRapidly ) // or ladder and moving up
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( bumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
			{
				ClearGroundEntity();
				moveToEndPos = false;

				if ( Velocity.z > 0 )
				{
					SurfaceFriction = 0.25f;
				}
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Position = pm.EndPosition;
			}
		}

		/// <summary>
		///     We have a new ground entity
		/// </summary>
		public virtual void UpdateGroundEntity( TraceResult tr )
		{
			GroundNormal = tr.Normal;

			// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
			// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
			// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
			SurfaceFriction = tr.Surface.Friction * 1.25f;
			if ( SurfaceFriction > 1 )
			{
				SurfaceFriction = 1;
			}

			GroundEntity = tr.Entity;

			if ( GroundEntity != null )
			{
				BaseVelocity = GroundEntity.Velocity;
			}
		}

		/// <summary>
		///     We're no longer on the ground, remove it
		/// </summary>
		public virtual void ClearGroundEntity()
		{
			if ( GroundEntity == null )
			{
				return;
			}

			// Clear on the pawn
			Pawn.GroundEntity = null;

			// Clear on the controller
			GroundEntity = null;

			GroundNormal = Vector3.Up;
			SurfaceFriction = 1.0f;
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
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.HitLayer( CollisionLayer.GRATE, true )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
				.WithoutTags( "player" )
				.Ignore( Pawn )
				.Run();

			tr.EndPosition -= TraceOffset;
			return tr;
		}

		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0 )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
				.Size( mins, maxs )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.HitLayer( CollisionLayer.GRATE, true )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
				.WithoutTags( "player" )
				.Ignore( Pawn )
				.Run();

			tr.EndPosition -= TraceOffset;
			return tr;
		}

		/// <summary>
		///     Try to keep a walking player on the ground when running down slopes etc
		/// </summary>
		public virtual void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * StepSize;

			// See how far up we can go without getting stuck
			var trace = TraceBBox( Position, start );
			start = trace.EndPosition;

			// Now trace down from a known safe position
			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 )
				return;

			if ( trace.Fraction >= 1 )
				return;

			if ( trace.StartedSolid )
				return;

			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle )
				return;

			Position = trace.EndPosition;
		}
	}
}
