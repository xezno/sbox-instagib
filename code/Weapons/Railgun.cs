namespace Instagib;

[Title( "Railgun" ), Icon( "sports_martial_arts" )]
[Library( "gib_weapon_railgun" )]
public partial class Railgun : BaseCarriable
{
	protected TimeSince TimeSinceDeployed { get; private set; }
	protected ICrosshair Crosshair { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/railgun/models/railgun.vmdl" );
		Tags.Add( "weapon" );
	}

	public override void CreateHudElements()
	{
		base.CreateHudElements();

		Crosshair = TypeLibrary.Create<ICrosshair>( "gib_crosshair_circle" );
	}

	public override void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( "weapons/railgun/models/railgun.vmdl" ) )
			return;

		ViewModelEntity = new ViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( "weapons/railgun/models/railgun.vmdl" );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		(ViewModelEntity as AnimatedEntity)?.SetAnimParameter( "deploy", true );
		TimeSinceDeployed = 0;
	}

	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

	public override void Simulate( IClient player )
	{
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}
		}

		if ( CanSecondaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary();
			}
		}
	}

	public virtual bool CanPrimaryAttack()
	{
		bool isFiring = Input.Pressed( InputButton.PrimaryAttack );

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = 0.75f;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void AttackPrimary()
	{
		ShootBullet();
	}

	public virtual void ShootBullet()
	{
		var tr = TraceBullet();

		//
		// Bullet damage etc.
		//
		if ( tr.Hit )
		{
			tr.Surface.DoBulletImpact( tr );
			if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
			{
				tr.Entity.TakeDamage( CreateDamageInfo( tr ) );
			}
		}

		//
		// Shoot effects
		//
		CreateShootEffects( tr.Direction, tr.EndPosition );

		using ( Prediction.Off() )
		{
			ViewModelEntity?.OnFire();
		}
	}

	protected virtual void CreateShootEffects( Vector3 direction, Vector3 endPosition )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

		if ( !effectEntity.IsValid() )
			return;

		var muzzleTransform = (effectEntity as ModelEntity)?.GetAttachment( "muzzle" ) ?? default;
		var startPosition = muzzleTransform.Position;

		var tracerParticles = Particles.Create( "particles/beam.vpcf", startPosition );
		tracerParticles.SetForward( 0, direction );
		tracerParticles.SetPosition( 1, endPosition );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( "railgun_fire" );
	}

	protected virtual DamageInfo CreateDamageInfo( TraceResult tr )
	{
		var damageInfo = DamageInfo
			.FromBullet( tr.EndPosition, tr.Direction * 32, 100 )
			.WithAttacker( Owner )
			.WithBone( tr.Bone )
			.WithWeapon( this );

		return damageInfo;
	}

	public virtual TraceResult TraceBullet()
	{
		float radius = 2.0f;

		var tr = Trace.Ray( Owner.AimRay, 8192f )
				.UseHitboxes()
				.WithAnyTags( "solid", "player" )
				.WithoutTags( "debris", "water", "clothing" )
				.Ignore( Owner )
				.Ignore( this )
				.Size( radius )
				.Run();

		return tr;
	}

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	public bool CanSecondaryAttack()
	{
		bool isFiring = Input.Pressed( InputButton.SecondaryAttack );

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = 1.0f;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	private void RocketJump( Vector3 pos, Vector3 normal )
	{
		ViewModelEntity?.OnFire();

		if ( !Game.IsServer )
			return;

		var sourcePos = pos;
		var radius = 64f;

		if ( Owner.Position.Distance( pos ) < radius )
		{
			var player = Owner as Player;
			var targetPos = player.PhysicsBody.MassCenter;
			var dir = (targetPos - sourcePos).Normal;
			var dist = dir.Length;

			var distaceMul = 1.0f - Math.Clamp( dist / radius, 0, 1 );
			var force = 400f * distaceMul;

			if ( player.Controller is QuakeWalkController quakeWalkController )
			{
				player.GroundEntity = null;
				quakeWalkController.GroundEntity = null;
			}

			player.Velocity += Vector3.Reflect( dir.WithZ( -16 ), normal ).Normal * force;
		}

		using ( Prediction.Off() )
		{
			var boostParticles = Particles.Create( "particles/boost.vpcf", pos );
			boostParticles.SetForward( 0, normal );
			PlaySound( "rocket_jump" );
		}
	}

	public void AttackSecondary()
	{
		var tr = Trace.Ray( AimRay, 256f )
				.UseHitboxes()
				.WithAnyTags( "solid", "player" )
				.WithoutTags( "debris", "water" )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1f )
				.Run();

		if ( !tr.Hit )
			return;

		RocketJump( tr.EndPosition, tr.Normal );
	}

	public virtual void RenderHud()
	{
		Crosshair?.RenderHud( TimeSincePrimaryAttack );
	}

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		base.SimulateAnimator( anim );

		anim.HoldType = CitizenAnimationHelper.HoldTypes.Shotgun;
	}
}
