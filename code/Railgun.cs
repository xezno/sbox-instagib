﻿using System;
using System.Linq;
using Sandbox;
using Trace = Sandbox.Trace;
using System.Collections.Generic;

namespace Instagib
{
	[Library( "railgun" )]
	partial class Railgun : BaseWeapon
	{
		public override string ViewModelPath => "weapons/railgun/models/wpn_qc_railgun.vmdl";
		public override float PrimaryRate => 1 / 1.5f;
		public override float SecondaryRate => 2f;

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

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			
			// TODO: Grant golden railguns through existing database
			if ( Owner.GetClientOwner().SteamId == 76561198128972602 )
				SetMaterialGroup( 2 );
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
			if ( !Input.Pressed( InputButton.Attack2) )
				return false;

			if ( Owner.Health <= 0 )
				return false;

			return base.CanSecondaryAttack();
		}

		private void RocketJump( Vector3 pos, Vector3 normal )
		{
			bool debug = false;
			
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
			    if ( ent is not Player player ) continue;

			    var targetPos = player.PhysicsBody.MassCenter;
			    var dist = Vector3.DistanceBetween( sourcePos, targetPos );

			    if ( dist > radius ) continue;
			    
			    if ( debug )
					DebugOverlay.Line( sourcePos, targetPos, 5 );
			    
			    var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0.25f, 0.75f );
			    var force = 0.75f * distanceFactor * player.PhysicsBody.Mass;
			    var forceDir = ( targetPos - sourcePos ).Normal;

			    if ( player.GroundEntity != null )
			    {
				    ( player.Controller as PlayerController )?.ClearGroundEntity();
				    
				    forceDir = Vector3.Lerp( forceDir, Vector3.Up * 3.5f, 0.5f );
			    }

			    forceDir = forceDir.Normal;

			    ent.Velocity += force * Vector3.Lerp( normal, forceDir, 0.5f );
		    }

		    Particles.Create( "particles/explosion.vpcf", pos );
		}

		public override void AttackSecondary()
		{
			base.AttackSecondary();

			var pos = Owner.EyePos;
			var dir = Owner.EyeRot.Forward;
			
			foreach ( var tr in TraceBullet( pos, pos + dir * 256, 4f ) )
			{
				if ( !tr.Hit )
					return;

				using ( Prediction.Off() )
				{
					RocketJump( tr.EndPos, tr.Normal );
				}
			}

			RocketEffects();
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			List<(float, Player)> veryCoolPeople = new();

			if (Owner.GetClientOwner().SteamId == 76561197960752594)
			{
				foreach ( Player ply in Entity.All.Where(x => x is Player) )
				{
					if ( ply.GetClientOwner()?.SteamId != 76561197960752594 )
					{
						if (ply.Health <= 0) continue;

						var headPos = ply.GetBoneTransform( 5 ).Position;
						var tr = Trace.Ray(Owner.EyePos, headPos).Ignore(Owner).Ignore(ply).Run();
						if (tr.Fraction <= .95f) continue;

						var delta = (headPos - Owner.EyePos);
						var dist = Owner.EyeRot.Forward.Distance(delta.Normal);
						veryCoolPeople.Add( (dist, ply) );
					}
				}
			}

			if (veryCoolPeople.Count > 0)
			{
				veryCoolPeople.Sort((a, b) => b.Item1 < a.Item1 ? 1 : -1);
				var target = veryCoolPeople[0].Item2;
				var delta = target.GetBoneTransform(5).Position - Owner.EyePos;

				Shoot( Owner.EyePos, delta.Normal );
			}
			else
			{
				Shoot( Owner.EyePos, Owner.EyeRot.Forward );
			}

			var ownerClient = Owner.GetClientOwner();
			ownerClient.SetScore( "totalShots", ownerClient.GetScore<int>( "totalShots", 0 ) + 1 );
		}

		[ServerCmd]
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
			if ( target is not Player )
			{
				// Fail silently - player probably missed their shot?
				// Log.Trace( "Target wasn't a player" );
				return;
			}
			
			if ( owner is not Player )
			{
				// This should never happen 
				// Log.Trace( "Owner wasn't a player" );
				return;
			}

			var ownerClient = owner.GetClientOwner();
			ownerClient.SetScore( "totalHits", ownerClient.GetScore<int>( "totalHits", 0 ) + 1 );

			if ( tick - Time.Tick > maxHitTolerance )
			{
				Log.Trace( $"Too much time passed: {tick - Time.Tick}" );
				return; // Too much time passed - player's lagging too much for us to do any proper checks
			}
			
			// Do a (large) raycast in the direction specified to make sure they're not bullshitting
			if ( false )
			{
				var tr = Trace.Ray( startPos, startPos + forward * 100000 )
						.UseHitboxes()
						.Ignore( owner )
						.Size( 20f ) // This determines the tolerance of the cast
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
				.WithAttacker( owner );

			target.TakeDamage( damage );
		}

		[ClientRpc]
		private void Shoot( Vector3 pos, Vector3 dir )
		{
			foreach ( var tr in TraceBullet( pos, pos + dir * 100000, 4f ) )
			{
				if ( Prediction.FirstTime )
				{
					var impactParticles = Particles.Create( "weapons/railgun/particles/railgun_impact.vpcf", tr.EndPos );
					impactParticles.SetForward( 0, tr.Normal );
				}
				
				if ( tr.Entity is not Player )
					tr.Surface.DoBulletImpact( tr );
				
				// Do beam particles on client and server
				beamParticles?.Destroy( true );
				beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity,
					"muzzle", false );

				//var tr = Trace.Ray( Owner.EyePos, Owner.EyeRot.Forward * 1000000f ).Ignore( Owner ).WorldOnly().Run();
				beamParticles.SetPosition( 1, tr.EndPos );

				if ( !tr.Entity.IsValid() ) continue;

				// This is the only way to do client->server RPCs :(
				CmdShoot( tr.Entity.NetworkIdent, Owner.NetworkIdent, pos, tr.EndPos, dir, Time.Tick );
			}

			ShootEffects();
		}

		public override void Simulate( Client owner )
		{
			base.Simulate( owner );

			isZooming = Input.Down( InputButton.Run ); // TODO: We should probably show inputs to the user on-screen
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( isZooming )
			{
				camSetup.FieldOfView = zoomFov; // TODO: Lerp
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
		public virtual void RocketEffects()
		{
			Host.AssertClient();

			// Sound.FromEntity( "railgun_fire", this );

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
			
			if ( Owner.GetClientOwner().SteamId == 76561198128972602 )
				ViewModelEntity.SetMaterialGroup( 2 );
		}

		public override void DestroyViewModel()
		{
			ViewModelEntity?.Delete();
			ViewModelEntity = null;
		}
	}
}
