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

	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

	public override void Simulate( Client player )
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

		//
		// Attack knockback
		//
		if ( IsServer )
		{
			if ( Owner is Player player )
			{
				player.Controller.ApplyImpulse( Owner.EyeRotation.Backward * 256f );
			}
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

		damageInfo.Flags |= DamageFlags.AlwaysGib;

		return damageInfo;
	}

	public virtual TraceResult TraceBullet()
	{
		Vector3 start = Owner.EyePosition;
		Vector3 end = Owner.EyePosition + Owner.EyeRotation.Forward * 8192f;

		float radius = 2.0f;

		bool inWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
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
		if ( InstagibGame.SelectedMoveSet != InstagibGame.MoveSet.Classic )
			return false;

		bool isFiring = Input.Pressed( InputButton.Run );

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = 1.0f;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	private void RocketJump( Vector3 pos, Vector3 normal )
	{
		ViewModelEntity?.OnFire();

		if ( !IsServer )
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
		var start = Owner.EyePosition;
		var end = start + Owner.EyeRotation.Forward * 256f;

		var tr = Trace.Ray( start, end )
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

	public virtual void RenderHud( Vector2 screenSize )
	{
		Crosshair?.RenderHud( TimeSincePrimaryAttack, screenSize );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_handedness", 0 );
	}
}
