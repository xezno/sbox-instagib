namespace Instagib;

[Title( "Player" ), Icon( "emoji_people" )]
public partial class Player : AnimatedEntity
{
	[Net, Predicted] public QuakeWalkController Controller { get; set; }
	[Net, Predicted] public Entity ActiveChild { get; set; }
	[Net] public float Shields { get; set; }

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public Entity LastActiveChild { get; set; }
	public DamageInfo LastDamageInfo { get; set; }

	public ModelEntity Corpse { get; set; }

	public TimeSince TimeSinceDied { get; private set; }
	private TimeSince timeSinceLastFootstep = 0;

	private ClothingContainer clothingContainer;

	public override Ray AimRay
	{
		get
		{
			return new Ray(
				Position + Vector3.Up * 64f,
				ViewAngles.Forward
			);
		}
	}

	public Vector3 EyePosition => AimRay.Position;
	public Rotation EyeRotation => Rotation.LookAt( AimRay.Forward );

	public Player()
	{

	}

	public Player( Client cl ) : this()
	{
		clothingContainer = new ClothingContainer();
		clothingContainer.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		EnableLagCompensation = true;

		Tags.Add( "player" );

		base.Spawn();
	}

	public void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableAllCollisions = true;

		Controller = new QuakeWalkController();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;

		CreateHull();
		ResetInterpolation();

		Dress();
	}

	private void Dress()
	{
		clothingContainer.DressEntity( this );
	}

	public override void OnKilled()
	{
		Client?.AddInt( "deaths", 1 );

		TimeSinceDied = 0;
		LifeState = LifeState.Dead;
		StopUsing();

		ActiveChild = null;
		LastActiveChild = null;

		EnableDrawing = false;
		EnableAllCollisions = false;
	}

	public virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		EnableHitboxes = true;
	}

	public override void BuildInput()
	{
		if ( !CanMove() )
		{
			Input.ClearButtons();
			Input.StopProcessing = true;
		}

		if ( Input.StopProcessing )
			return;

		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;

		if ( IsZooming )
			viewAngles = Angles.Lerp( viewAngles, ViewAngles, 0.5f );

		ViewAngles = viewAngles.Normal;

		ActiveChild?.BuildInput();
		Controller?.BuildInput();
	}

	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();
		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public virtual float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 0.2f;
	}

	public override void StartTouch( Entity other )
	{
		if ( IsClient ) return;

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );
			return;
		}
	}

	public virtual void SimulateActiveChild( Client cl, Entity child )
	{
		if ( LastActiveChild != ActiveChild )
		{
			if ( LastActiveChild is BaseCarriable previousBc )
				previousBc?.ActiveEnd( Owner, previousBc.Owner != Owner );

			if ( ActiveChild is BaseCarriable nextBc )
				nextBc?.ActiveStart( Owner );
		}

		LastActiveChild = ActiveChild;

		if ( !ActiveChild.IsValid() )
			return;

		if ( ActiveChild.IsAuthority )
			ActiveChild.Simulate( cl );
	}

	void SimulateAnimation( PawnController controller )
	{
		if ( controller == null )
			return;

		var turnSpeed = 0.02f;

		Rotation rotation;

		if ( Client.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsNoclipping = controller.HasTag( "noclip" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();

		if ( ActiveChild is BaseCarriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}
	}
}
