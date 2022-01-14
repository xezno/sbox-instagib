using Sandbox;
using System.Linq;
using Instagib.UI;
using Event = Sandbox.Event;
using Instagib.Weapons;

namespace Instagib
{
	public partial class Player : Sandbox.Player
	{
		[Net, Local] private TimeSince TimeSinceSpawn { get; set; }
		public bool IsSpawnProtected => TimeSinceSpawn < 3;

		private Particles speedLines;

		private DamageInfo lastDamageInfo;

		//
		// Stats used for medals
		//
		public int CurrentStreak { get; set; }
		public float CurrentDamageDealt { get; set; }

		public Clothing.Container Clothing = new();

		public Player()
		{
			Inventory = new BaseInventory( this );
		}

		public Player( Client cl ) : this()
		{
			Clothing.LoadFromClient( cl );
		}

		public override void Spawn()
		{
			base.Spawn();
			EnableLagCompensation = true;
		}

		public override void Respawn()
		{
			Event.Run( "playerRespawn" );

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new PlayerController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Transmit = TransmitType.Always;

			Tags.Add( "player" );

			Clothing.DressEntity( this );

			Inventory.DeleteContents();
			Inventory.Add( new Railgun(), true );

			CurrentStreak = 0;
			CurrentDamageDealt = 0;
			TimeSinceSpawn = 0;

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );

			if ( Position.z < -2500 )
			{
				TakeDamage( DamageInfo.Generic( 10000 ) );
			}

			if ( cl == Local.Client )
			{
				GlowActive = false;

				//
				// Speed lines
				//
				if ( IsClient && Velocity.Length > 500 )
				{
					speedLines ??= Particles.Create( "particles/speed_lines.vpcf" );
					var perlinStrength = Velocity.Length.LerpInverse( 500, 700 ) * 0.5f;
					_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, perlinStrength, 2.0f * perlinStrength );
				}
				else if ( IsClient && Velocity.Length < 600 && speedLines != null )
				{
					speedLines?.Destroy();
					speedLines = null;
				}

				return;
			}

			GlowActive = true;
			GlowState = GlowStates.GlowStateOn;
			GlowDistanceStart = -32;
			GlowDistanceEnd = 4096;
		}

		[Event.Tick.Client]
		public void OnClientTick()
		{
			// Outlines are per-client. They can be disabled and recolored at each clients' will
			// Note that this isn't synced between clients at all

			if ( IsServer )
				return;

			var hsvColor = Color.Red.ToHsv();

			if ( IsFriendly( Local.Client ) )
				hsvColor = Color.Cyan.ToHsv();

			hsvColor.Value = 1.0f;
			hsvColor.Saturation = 1.0f;
			GlowColor = hsvColor.ToColor();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Velocity = Vector3.Zero;

			Camera = new LookAtCamera();
			var lookAtCamera = Camera as LookAtCamera;

			lookAtCamera.TargetEntity = LastAttacker;
			lookAtCamera.Origin = EyePos;
			lookAtCamera.Rotation = EyeRot;
			lookAtCamera.TargetOffset = Vector3.Up * 64f;

			Inventory.DeleteContents();

			EnableDrawing = false;
			EnableAllCollisions = false;

			_ = Particles.Create( "particles/gib_blood.vpcf", Position + (Vector3.Up * (8)) );

			ShakeScreen( To.Everyone, Position );

			Sound.FromWorld( "gibbing", Position );

			var childrenCopy = Children.ToList();
			foreach ( var child in childrenCopy )
			{
				if ( !child.Tags.Has( "clothes" ) ) continue;
				if ( child is not ModelEntity e ) continue;

				e.Delete();
			}
		}

		[ClientRpc]
		private void ShakeScreen( Vector3 position )
		{
			float strengthMul = 10f;
			float strengthDist = 512f;

			float strength = strengthDist - Local.Pawn.Position.Distance( position ).Clamp( 0, strengthDist );
			strength /= strengthDist;
			strength *= strengthMul;

			_ = new Sandbox.ScreenShake.Perlin( 1f, 0.75f, strength );
		}

		[ClientRpc]
		public void OnDamageOther( Vector3 pos, float amount )
		{
			using ( Prediction.Off() )
			{
				Log.Trace( "Playing kill sound" );
				PlaySound( "kill" );
				Hitmarker.CurrentHitmarker.OnHit();
			}
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( IsSpawnProtected || info.Flags.HasFlag( DamageFlags.PhysicsImpact ) )
				return;

			if ( IsFriendly( info.Attacker ) )
				return;

			lastDamageInfo = info;

			base.TakeDamage( info );

			if ( info.Attacker is Player attacker )
			{
				attacker.OnDamageOther( To.Single( attacker ), info.Position, info.Damage );
			}
		}
	}
}
