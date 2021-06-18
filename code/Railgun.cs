﻿using System;
using System.Linq;
using Sandbox;

namespace Instagib
{
	[Library( "railgun" )]
	partial class Railgun : BaseWeapon
	{
		public override string ViewModelPath => "weapons/railgun/models/wpn_qc_railgun.vmdl";
		public override float PrimaryRate => 1 / 1.5f;

		private const float maxHitTolerance = (90f / 1000f) * 500; // (tickrate / 1000ms) * desired_ms, number of ticks tolerance given to a hit. 
		private float zoomFov = 60f;

		private Particles beamParticles;
		private bool isZooming;

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
			if ( !Input.Pressed( InputButton.Attack1 ) )
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

		// This should ideally not be a user-invoked command
		[ServerCmd( "send_shoot" )]
		private static void CmdShoot( int targetIdent, int ownerIdent, Vector3 startPos, Vector3 endPos, Vector3 forward, int tick )
		{
			Host.AssertServer();

			var target = Entity.All.First( e => e.NetworkIdent == targetIdent );
			var owner = Entity.All.First( e => e.NetworkIdent == ownerIdent );

			//
			// Checking:
			// In order to prevent people from just typing stuff like "shoot Alex", we'll do some light checking to
			// verify stuff
			//	
			if ( target is not InstagibPlayer )
			{
				// Fail silently - player probably missed their shot?
				Log.Trace( "Target wasn't a player" );
				return;
			}

			if ( owner is not Player )
			{
				// This should never happen 
				Log.Trace( "Owner wasn't a player" );
				return;
			}

			if ( tick - Time.Tick > maxHitTolerance )
			{
				Log.Trace( $"Too much time passed: {tick - Time.Tick}" );
				return; // Too much time passed - player's lagging too much for us to do any proper checks
			}

			// Do a (large) raycast in the direction specified to make sure they're not bullshitting
			{
				var tr = Trace.Ray( startPos, startPos + forward * 100000 )
						.UseHitboxes()
						.Ignore( owner )
						.Size( 25f ) // This determines the tolerance of the cast; 25 is a good value for most playable pings
						.EntitiesOnly()
						.Run();

				if ( !tr.Hit )
				{
					Log.Trace( "Didn't hit" );
					return;
				}

				if ( !tr.Entity.IsValid() )
				{
					Log.Trace( "Entity invalid" );
					return;
				}

				if ( tr.Entity.NetworkIdent != targetIdent )
				{
					Log.Trace( "Idents didnt match" );
					return;
				}
			}

			//
			// Damage
			//
			var damage = DamageInfo.FromBullet( endPos, forward.Normal * 20, 1000 )
				.WithAttacker( owner ).WithWeapon( new Railgun() );

			target.TakeDamage( damage );
		}

		[ClientRpc]
		private void Shoot( Vector3 pos, Vector3 dir )
		{
			foreach ( var tr in TraceBullet( pos, pos + dir * 100000, 12.5f ) )
			{
				tr.Surface.DoBulletImpact( tr );

				// Do beam particles on client and server
				beamParticles?.Destroy( true );
				beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity,
					"muzzle" );

				if ( !tr.Entity.IsValid() ) continue;

				// This is the only way to do client->server RPCs :(
				ConsoleSystem.Run( "send_shoot", tr.Entity.NetworkIdent, Owner.NetworkIdent, pos, tr.EndPos, dir, Time.Tick );
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

			isZooming = Input.Down( InputButton.View );
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( isZooming )
			{
				camSetup.FieldOfView = zoomFov;
			}
		}
		
		public override void BuildInput( InputBuilder owner ) 
		{
			if ( isZooming )
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
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 1.0f, 4.0f );
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
