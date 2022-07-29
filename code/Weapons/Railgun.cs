namespace Instagib;

[Title( "Instagib Railgun" ), Icon( "sports_martial_arts" )]
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

		Crosshair = TypeLibrary.Create<ICrosshair>( "gib_crosshair_dot" );
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

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

	[Net, Predicted]
	public TimeSince TimeSinceAttack { get; set; }

	public override void Simulate( Client player )
	{
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceAttack = 0;
				AttackPrimary();
			}
		}
	}

	public virtual bool CanPrimaryAttack()
	{
		bool isFiring = Input.Pressed( InputButton.PrimaryAttack );

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = 0.75f;
		if ( rate <= 0 ) return true;

		return TimeSinceAttack > (1 / rate);
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
			var fireSound = Path.GetFileNameWithoutExtension( "railgun_fire" );
			PlaySound( fireSound );

			ViewModelEntity?.OnFire();
		}
	}

	protected virtual void CreateShootEffects( Vector3 direction, Vector3 endPosition )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

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
			.WithBone( tr.Bone );

		damageInfo.Flags |= DamageFlags.AlwaysGib;

		return damageInfo;
	}

	public virtual TraceResult TraceBullet()
	{
		using var _ = LagCompensation();

		Vector3 start = Owner.EyePosition;
		Vector3 end = Owner.EyePosition + Owner.EyeRotation.Forward * 8192f;

		float radius = 2.0f;

		bool inWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player" )
				.WithoutTags( "debris", "water" )
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

	public virtual void RenderHud( Vector2 screenSize )
	{
		Crosshair?.RenderHud( TimeSinceAttack, screenSize );
	}
}
