using Sandbox;
using System;
using System.Linq;
using Instagib.UI;

namespace Instagib
{
	public partial class InstagibPlayer : Player
	{
		private DamageInfo lastDamageInfo;
		
		//
		// Stats used for medals
		//
		public int CurrentStreak { get; set; }
		public float TotalDamageDealt { get; set; }
		public float CurrentDamageDealt { get; set; }
		
		//
		// Dynamic hud / camera
		//
		private Vector3 lastCameraPos = Vector3.Zero;
		private Rotation lastCameraRot = Rotation.Identity;
		private float lastHudOffset;

		public InstagibPlayer()
		{
			Inventory = new BaseInventory( this );
		}

		[ClientRpc]
		private void AssignSettings()
		{
			if ( Camera is FirstPersonCamera camera )
				camera.defaultFov = Cookie.Get( "Instagib.Fov", 90 );

			ViewModel.Offset = Cookie.Get<float>( "Instagib.ViewmodelOffset", 0 );
			ViewModel.Visible = Cookie.Get( "Instagib.ViewmodelVisible", true );
			ViewModel.Flip = Cookie.Get( "Instagib.ViewmodelFlip", false );
				
			Crosshair.Visible = Cookie.Get( "Instagib.CrosshairVisible", true );
			Crosshair.SetCrosshair( Cookie.Get( "Instagib.CrosshairGlyph", "a" ));
		}
		
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new InstagibController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Dress();

			Inventory.Add( new Railgun(), true );

			CurrentStreak = 0;
			CurrentDamageDealt = 0;

			AssignSettings();
			
			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );
			TickPlayerUse();

			if ( cl == Local.Client )
			{
				GlowActive = false;
				return;
			}
			
			GlowActive = true;
			GlowState = GlowStates.GlowStateOn;
			GlowDistanceStart = -32;
			GlowDistanceEnd = 4096;
			GlowColor = Color.Red.WithAlpha( 0.5f );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
			EnableAllCollisions = false;
			
			_ = Particles.Create( "particles/gib_blood.vpcf", Position + (Vector3.Up * (8)) );
			
			Sound.FromWorld( "gibbing", Position );
			Camera = new SpectateRagdollCamera();
		}

		[ClientRpc]
		public void OnDamageOther( Vector3 pos, float amount )
		{
			Log.Trace( $"{Local.DisplayName} damaged another player" );
			
			PlaySound( "kill" );
			
			Hitmarker.CurrentHitmarker.OnHit();
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );
			lastDamageInfo = info;

			if ( info.Attacker is InstagibPlayer attacker )
			{
				attacker.OnDamageOther( To.Single( attacker ), info.Position, info.Damage );
			}
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( lastCameraRot == Rotation.Identity )
				lastCameraRot = setup.Rotation;

			var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 10.0f;

			if ( angleDiffDegrees > allowance )
				lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );

			if ( setup.Viewer != null )
				AddCameraEffects( ref setup );
		}

		float walkBob = 0;
		float lean = 0;
		float fov = 0;

		private void AddCameraEffects( ref CameraSetup setup )
		{
			var speed = Velocity.Length.LerpInverse( 0, 320 );
			var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			if ( GroundEntity != null )
			{
				walkBob += Time.Delta * 25.0f * speed;
			}

			setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
			setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

			// Camera lean
			var leanMax = 0.015f;
			var leanMul = 0.005f;
			var leanSmooth = 15.0f;
			
			lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * leanMul, Time.Delta * leanSmooth );
			lean.Clamp( -leanMax, leanMax );

			var appliedLean = lean;
			appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
			setup.Rotation *= Rotation.From( 0, 0, appliedLean );

			speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

			fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

			setup.FieldOfView += fov;

			var tx = new Sandbox.UI.PanelTransform();
			tx.AddRotation( 0, 0, lean * -0.2f ); 
			var zOffset = (lastCameraPos - setup.Position).z * 2f;
			zOffset = lastHudOffset.LerpTo( zOffset, 25.0f * Time.Delta );
			tx.AddTranslateY( zOffset );

			lastHudOffset = zOffset;

			if ( InstagibHud.TiltingHudPanel != null )
			{
				InstagibHud.TiltingHudPanel.Style.Transform = tx;
				InstagibHud.TiltingHudPanel.Style.Dirty();
			}

			lastCameraPos = setup.Position;
		}

	}
}
