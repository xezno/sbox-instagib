using System;
using System.Collections.Generic;
using Sandbox;
using Trace = Sandbox.Trace;

namespace Instagib.Weapons
{
	[Library( "railgun" )]
	partial class Railgun : BaseWeapon
	{
		public ViewModel ViewModel => (ViewModelEntity as ViewModel) ?? default;

		public override string ViewModelPath => "weapons/railgun/models/railgun.vmdl";
		public override float PrimaryRate => 0.75f;
		public override float SecondaryRate => 2f;

		private Particles beamParticles;
		public bool IsZooming { get; private set; }

		public override void Reload() { }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/railgun/models/railgun.vmdl" );
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
			ViewModel?.OnFire();

			var sourcePos = pos;
			var radius = 128;
			var overlaps = Physics.GetEntitiesInSphere( sourcePos, radius );

			foreach ( var overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent || !ent.IsValid() ) continue;
				if ( ent != Owner ) continue;
				if ( ent.LifeState != LifeState.Alive || !ent.PhysicsBody.IsValid() || ent.IsWorld ) continue;

				var dir = normal.Normal;
				var dist = dir.Length;

				if ( dist > radius ) continue;

				var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0, 1 );
				distanceFactor *= 0.5f;
				var force = distanceFactor * ent.PhysicsBody.Mass;

				if ( ent.GroundEntity != null )
				{
					ent.GroundEntity = null;
					if ( ent is Player { Controller: PlayerController playerController } )
						playerController.ClearGroundEntity();
				}

				ent.ApplyAbsoluteImpulse( dir * force );
			}

			using ( Prediction.Off() )
			{
				Particles.Create( "particles/explosion.vpcf", pos );
				PlaySound( "rocket_jump" );
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

				RocketJump( tr.EndPos, tr.Normal );
			}

			RocketEffects();
		}

		public override void AttackPrimary()
		{
			ViewModel?.OnFire();

			TimeSincePrimaryAttack = 0;

			using ( Prediction.Off() )
			{
				Owner.Client.AddInt( "totalShots" );
			}

			Shoot( Owner.EyePos, Owner.EyeRot.Forward );
		}

		public IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 dir, float radius = 2.0f, float dist = 100000f )
		{
			using ( LagCompensation() )
			{
				bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

				var end = start + dir * dist;
				var tr = Trace.Ray( start, end )
					.HitLayer( CollisionLayer.Water, !InWater )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

				yield return tr;
			}
		}

		private void Shoot( Vector3 pos, Vector3 dir )
		{
			foreach ( var tr in TraceBullet( pos, dir, 8f, 8192f ) )
			{
				if ( Prediction.FirstTime )
				{
					var impactParticles =
						Particles.Create( "weapons/railgun/particles/railgun_impact.vpcf", tr.EndPos );
					impactParticles.SetForward( 0, tr.Normal );
				}

				if ( tr.Entity is not Player )
				{
					tr.Surface.DoBulletImpact( tr );
				}

				// Particles
				{
					beamParticles?.Destroy( true );
					beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity,
						"muzzle", false );

					beamParticles?.SetPosition( 1, tr.EndPos );

					float particleCount = tr.Distance / 128f;
					beamParticles?.SetPosition( 2, particleCount );
				}

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPos, dir * 1000, 1000 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
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
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 1.0f, 2.0f, 2.0f );
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
	}
}
