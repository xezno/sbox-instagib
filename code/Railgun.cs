using System;
using Sandbox;

[Library( "railgun" )]
partial class Railgun : BaseWeapon
{
	public override string ViewModelPath => "weapons/railgun/models/v_railgun.vmdl";
	public override float PrimaryRate => 1/1.5f;

	private Particles beamParticles;

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
		if ( !Owner.Input.Pressed( InputButton.Attack1 ) )
			return false;

		if ( Owner.Health <= 0 )
			return false;

		return base.CanPrimaryAttack();
	}

	public override void Reload() { }

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Shoot( Owner.EyePos, Owner.EyeRot.Forward );
	}

	private void Shoot( Vector3 pos, Vector3 dir )
	{
		var forward = dir * 10000;
		
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		bool hit = false;
		foreach ( var tr in TraceBullet( pos, pos + dir * 4000 ) )
		{
			tr.Surface.DoBulletImpact( tr );
			
			ShootEffects( tr.EndPos );
			hit = true;
			
			// Do beam particles on client and server
			beamParticles?.Destroy( true );
			beamParticles = Particles.Create( "weapons/railgun/particles/railgun_beam.vpcf", EffectEntity, "muzzle" );

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
		
		if ( !hit )
			ShootEffects( pos + forward );
	}

	public override void Simulate( Client owner )
	{
		base.Simulate( owner );

		if ( beamParticles != null )
		{
			var tr = Trace.Ray( Owner.EyePos, Owner.EyeRot.Forward * 1000000f ).Ignore( Owner ).WorldOnly().Run();
			beamParticles.SetPos( 1, tr.EndPos );
		}
	}

	[ClientRpc]
	public virtual void ShootEffects( Vector3 direction )
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
	

	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
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

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	public override void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}
}
