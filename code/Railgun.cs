using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Instagib.Utils;
using Sandbox;
using Trace = Sandbox.Trace;

namespace Instagib
{
	[Library( "railgun" )]
	partial class Railgun : BaseWeapon
	{
		public override string ViewModelPath => "weapons/railgun/models/railgun.vmdl";
		public override float PrimaryRate => 1 / 1.5f;
		public override float SecondaryRate => 2f;

		private static float MaxHitTolerance =>
			(Global.TickRate / 1000f) *
			1000; // (tickrate / 1000ms) * desired_ms, number of ticks tolerance given to a hit.

		private Particles beamParticles;
		public bool IsZooming { get; private set; }

		public override void Reload() { } // No reload

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/railgun/models/railgun.vmdl" ); // TODO: LOD
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			base.SimulateAnimator( anim );
			anim.SetParam( "holdtype", 3 );
		}

		public override bool CanPrimaryAttack()
		{
			if ( !Input.Pressed( InputButton.Attack1 ) )
				return false;

			if ( Owner.Health <= 0 )
				return false;

			return base.CanPrimaryAttack();
		}

		public override bool CanSecondaryAttack()
		{
			if ( !Input.Pressed( InputButton.Attack2 ) )
				return false;

			if ( Owner.Health <= 0 )
				return false;

			return base.CanSecondaryAttack();
		}

		private void RocketJump( Vector3 pos, Vector3 normal )
		{
			bool debug = false;

			// Grapple reset
			(ViewModelEntity as ViewModel)?.OnFire();
			TimeSinceLastGrapple = 100;

			if ( !IsServer )
				return;

			var sourcePos = pos;
			var radius = 128;
			var overlaps = Physics.GetEntitiesInSphere( sourcePos, radius );

			if ( debug )
				DebugOverlay.Sphere( pos, radius, Color.Yellow, true, 5f );

			foreach ( var overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent || !ent.IsValid() ) continue;
				if ( ent.LifeState != LifeState.Alive && !ent.PhysicsBody.IsValid() && ent.IsWorld ) continue;
				if ( ent.PhysicsBody == null ) continue;
				if ( ent.IsWorld ) continue;

				var targetPos = ent.PhysicsBody.MassCenter;
				var dir = (targetPos - sourcePos).Normal;
				var dist = dir.Length;

				if ( dist > radius ) continue;

				if ( debug )
					DebugOverlay.Line( sourcePos, targetPos, 5 );

				var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0, 1 );
				distanceFactor *= 0.5f;
				var force = distanceFactor * ent.PhysicsBody.Mass;
				
				if ( ent.GroundEntity != null )
				{
					ent.GroundEntity = null;
					if ( ent is Player { Controller: PlayerController playerController } )
						playerController.ClearGroundEntity();
				}

				if ( ent is not Player )
					force *= 10;

				ent.Velocity += Vector3.Reflect( dir.WithZ( -32 ), normal ).Normal * force;
				// ent.Velocity += force * Vector3.Lerp( normal, forceDir, 0.5f );
			}

			using ( Prediction.Off() )
			{
				Particles.Create( "particles/explosion.vpcf", pos );
			}
		}

		public override void AttackSecondary()
		{
			base.AttackSecondary();

			var pos = Owner.EyePos;
			var dir = Owner.EyeRot.Forward;

			foreach ( var tr in TraceBullet( pos, dir, 1f, 256f ) )
			{
				if ( !tr.Hit )
					return;

				// DebugOverlay.Line( tr.StartPos, tr.EndPos, 10f );

				RocketJump( tr.EndPos, tr.Normal );
			}

			RocketEffects();
		}

		public override void AttackPrimary()
		{
			(ViewModelEntity as ViewModel)?.OnFire();

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			Shoot( Owner.EyePos, Owner.EyeRot.Forward );

			var ownerClient = Owner.GetClientOwner();
			ownerClient.AddInt( "totalShots" );
		}

		[ServerCmd]
		private static void CmdShoot( int targetIdent, int ownerIdent, Vector3 startPos, Vector3 endPos,
			Vector3 forward, int tick, int hitbox )
		{
			Host.AssertServer();

			var target = Entity.All.First( e => e.NetworkIdent == targetIdent );
			var owner = Entity.All.First( e => e.NetworkIdent == ownerIdent );

			//
			// Checking:
			// In order to prevent people from just typing stuff like "shoot Alex", we'll do some light checking to
			// verify stuff
			//	
			if ( target is Player )
			{
				var ownerClient = owner.GetClientOwner();
				ownerClient.AddInt( "totalHits" );

				if ( tick - Time.Tick > MaxHitTolerance )
				{
					// Too much time passed - player's lagging too much for us to do any proper checks
					Log.Trace( $"Too much time passed: {tick - Time.Tick}" );
					return;
				}
			}

			if ( owner is not Player )
			{
				Log.Trace( $"Owner {owner.EngineEntityName} ({ownerIdent} isn't player" );
				return;
			}

			//
			// Damage
			//
			var damage = DamageInfo.FromBullet( endPos, forward.Normal * 100 * 1000, 1000 )
				.WithAttacker( owner ).WithHitbox( hitbox );

			target.TakeDamage( damage );
		}

		/// <summary>
		/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
		/// hits, like if you're going through layers or ricocet'ing or something.
		/// </summary>
		public IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 dir, float radius = 2.0f, float dist = 100000f )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var end = start + dir * dist;
			var tr = Trace.Ray( start, end )
				//.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Owner )
				.Ignore( this )
				.Size( radius )
				.Run();

			yield return tr;
			//
			// Another trace, bullet going through thin material, penetrating water surface?
			//
		}

		[ClientRpc]
		private void Shoot( Vector3 pos, Vector3 dir )
		{
			foreach ( var tr in TraceBullet( pos, dir, 8f, 100000f ) )
			{
				// DebugOverlay.Line( tr.StartPos, tr.EndPos, 5f );
				// DebugOverlay.Circle( tr.EndPos, Rotation.From( tr.Normal.EulerAngles ), 8f, Color.Red, false, 5f );
				
				if ( Prediction.FirstTime )
				{
					var impactParticles =
						Particles.Create( "weapons/railgun/particles/railgun_impact.vpcf", tr.EndPos );
					impactParticles.SetForward( 0, tr.Normal );
				}

				if ( tr.Entity is not Player )
					tr.Surface.DoBulletImpact( tr );

				// Do beam particles on client and server
				beamParticles?.Destroy( true );
				beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity,
					"muzzle", false );

				beamParticles.SetPosition( 1, tr.EndPos );

				if ( !tr.Entity.IsValid() ) continue;

				// This is the only way to do client->server RPCs :(
				if ( IsClient )
					CmdShoot( tr.Entity.NetworkIdent, Owner.NetworkIdent, pos, tr.EndPos, dir, Time.Tick,
						tr.HitboxIndex );
			}

			ShootEffects();
		}

		public override void Simulate( Client owner )
		{
			base.Simulate( owner );

			IsZooming = Input.Down( InputButton.Run );
			GrappleSimulate( owner );
		}

		public override void BuildInput( InputBuilder owner )
		{
			if ( IsZooming )
			{
				// Set input sensitivity
				owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles,
					PlayerSettings.ZoomedFov / PlayerSettings.Fov );
			}
		}

		[ClientRpc]
		public virtual void RocketEffects()
		{
			Host.AssertClient();

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "onattack" );

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 2.0f );
			}
		}

		[ClientRpc]
		public virtual void ShootEffects()
		{
			Host.AssertClient();

			Sound.FromEntity( "railgun_fire", this );

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "onattack" );

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 4.0f );
			}
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void DestroyViewModel()
		{
			ViewModelEntity?.Delete();
			ViewModelEntity = null;
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );
			RemoveGrapple();
		}
	}
}
