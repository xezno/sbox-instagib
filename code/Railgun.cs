using System;
using Sandbox;

namespace Instagib
{
	[Library( "railgun" )]
	partial class Railgun : BaseWeapon
	{
		public override string ViewModelPath => "weapons/railgun/models/wpn_qc_railgun.vmdl";
		public override float PrimaryRate => 1 / 1.5f;

		private float zoomFov = 60f;

		private Particles beamParticles;
		private bool zoom;

		public override void Reload() { } // No reload

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/railgun/models/wpn_qc_railgun.vmdl" ); // TODO: LOD
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			base.SimulateAnimator( anim );
			anim.SetParam( "holdtype", 3 );
		}

		public override bool CanPrimaryAttack()
		{
			if ( !Owner.Input.Pressed( InputButton.Attack1 ) )
				return false;

			if ( Owner.Health <= 0 )
				return false;

			return base.CanPrimaryAttack();
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			Shoot( Owner.EyePos, Owner.EyeRot.Forward );
		}

		private void Shoot( Vector3 pos, Vector3 dir )
		{
			var forward = dir * 10000;

			foreach ( var tr in TraceBullet( pos, pos + dir * 100000, 10f ) )
			{
				tr.Surface.DoBulletImpact( tr );

				// Do beam particles on client and server
				beamParticles?.Destroy( true );
				beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity,
					"muzzle" );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				using ( Prediction.Off() )
				{
					var damage = DamageInfo.FromBullet( tr.EndPos, forward.Normal * 20, 1000 )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damage );
				}
			}

			ShootEffects();
		}

		public override void Simulate( Client owner )
		{
			base.Simulate( owner );

			if ( beamParticles != null )
			{
				var tr = Trace.Ray( Owner.EyePos, Owner.EyeRot.Forward * 1000000f ).Ignore( Owner ).WorldOnly().Run();
				beamParticles.SetPos( 1, tr.EndPos );
			}

			zoom = Input.Down( InputButton.Attack2 );
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( zoom )
			{
				camSetup.FieldOfView = zoomFov;
			}
		}
		
		public override void BuildInput( InputBuilder owner ) 
		{
			if ( zoom )
			{
				// Half input sensitivity
				owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, zoomFov / 90f );
			}
		}

		[ClientRpc]
		public virtual void ShootEffects()
		{
			Host.AssertClient();

			Sound.FromEntity( "railgun_fire", this );

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.OnEvent( "onattack" );

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 1.0f, 1.0f );
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
