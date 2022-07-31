namespace Instagib;

partial class Player
{
	[Net] private bool IsInvincible { get; set; }
	[ConVar.Replicated( "gib_debug_player" )] public static bool Debug { get; set; }
	private Announcer Announcer { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if ( IsLocalPawn )
		{
			Announcer = new();
		}
	}

	public override void Simulate( Client cl )
	{
		if ( LifeState == LifeState.Dead )
			return;

		Corpse?.Delete();

		Controller?.Simulate( cl, this, Animator );
		SimulateCheckOutOfBounds( cl );
		SimulateActiveChild( cl, ActiveChild );
		SimulatePlayerUse( cl );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( CameraMode is FirstPersonCamera )
				CameraMode = new ThirdPersonCamera();
			else
				CameraMode = new FirstPersonCamera();
		}
	}

	private void SimulateCheckOutOfBounds( Client cl )
	{
		if ( !IsServer )
			return;

		if ( Position.z < -1000 )
			TakeDamage( DamageInfo.Generic( 10000f ) );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );

		if ( Debug )
		{
			DebugOverlay.ScreenText( "[PLAYER]\n" +
				$"ActiveChild:                 {ActiveChild}\n" +
				$"LastActiveChild:             {LastActiveChild}\n" +
				$"Health:                      {Health}\n" +
				$"God:                         {IsInvincible}",
				new Vector2( 60, 150 ) );
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		LastDamageInfo = info;

		if ( IsInvincible && info.Attacker.IsValid() )
			return;

		float newHealth = Health - info.Damage;

		base.TakeDamage( info );

		// Do a few things if this killed the player
		if ( LifeState == LifeState.Dead )
		{
			// Tell ourselves that we died
			RpcOnDeath( To.Single( this ), info.Attacker?.NetworkIdent ?? -1 );

			// Tell attacker that they killed us
			if ( info.Attacker != null && info.Attacker.Client != null && info.Attacker != this )
			{
				RpcOnKill( To.Single( info.Attacker ), this.NetworkIdent );

				info.Attacker.Client.AddInt( "kills" );
			}

			// Gibbing & ragdolling
			bool shouldGib = newHealth <= -50;

			if ( info.Flags.HasFlag( DamageFlags.AlwaysGib ) )
				shouldGib = true;

			if ( info.Flags.HasFlag( DamageFlags.DoNotGib ) )
				shouldGib = false;

			if ( shouldGib )
			{
				// Sometimes we might not get a damage position (i.e. if it was through
				// an explosive or trigger) so we'll take the player's position and move
				// up a little bit to make things still look okay
				if ( info.Position == Vector3.Zero )
					info.Position = Position + new Vector3( 0, 0, 32 );

				BecomeGibsOnClient( To.Everyone, info.Position );
			}
			else
			{
				BecomeRagdollOnClient( To.Everyone );
			}
		}
		else
		{
			this.ProceduralHitReaction( info );
		}

		// Tell attacker that they did damage to us
		if ( IsServer && info.Attacker != this )
		{
			RpcDamageDealt( To.Single( info.Attacker ),
				LifeState == LifeState.Dead, info.Position, info.Damage, NetworkIdent );
		}
	}

	public bool CanMove()
	{
		return true;
	}

	[ClientRpc]
	public void RpcOnKill( int victimIdent )
	{
		var victim = Entity.All.OfType<Player>().FirstOrDefault( x => x.NetworkIdent == victimIdent );
		Event.Run( InstagibEvent.Player.Kill.Name, victim, LastDamageInfo );
	}

	[ClientRpc]
	public void RpcOnDeath( int attackerIdent )
	{
		var attacker = Entity.All.OfType<Player>().FirstOrDefault( x => x.NetworkIdent == attackerIdent );
		Event.Run( InstagibEvent.Player.Death.Name, attacker, LastDamageInfo );
	}

	[ClientRpc]
	public void RpcDamageDealt( bool isKill, Vector3 position, float damageAmount, int victimNetworkId )
	{
		var victim = All.OfType<Player>().First( x => x.NetworkIdent == victimNetworkId );
		Log.Trace( $"We did damage to {victim}" );

		if ( isKill )
			PlaySound( "kill" );

		float t = damageAmount.LerpInverse( 0.0f, 100.0f );
		float pitch = MathX.LerpTo( 1.0f, 1.25f, t );

		// TODO(AG) : Am I fucking something up or does SetPitch not work
		Sound.FromScreen( "hit" ).SetRandomPitch( pitch, pitch );

		Event.Run( InstagibEvent.Player.DidDamage.Name, position, damageAmount );
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( ActiveChild is Railgun railgun )
		{
			railgun.RenderHud( screenSize );
		}
	}
}
