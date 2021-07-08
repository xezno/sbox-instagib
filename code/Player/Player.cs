﻿using Sandbox;
using System;
using System.Linq;
using Instagib.UI;
using Sandbox.ScreenShake;
using Instagib.Utils;

namespace Instagib
{
	public partial class Player : Sandbox.Player
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

		public Player()
		{
			Inventory = new BaseInventory( this );
		}
		
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new PlayerController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Tags.Add( "player" );

			Dress();

			Inventory.Add( new Railgun(), true );

			CurrentStreak = 0;
			CurrentDamageDealt = 0;
			
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
		}

		[Event.Tick.Client]
		public void OnClientTick()
		{
			// Outlines are per-client. They can be disabled and recolored at each clients' will
			// Note that this isn't synced between clients at all

			if ( IsServer )
				return;

			var hsvColor = PlayerSettings.EnemyOutlineColor.ToHsv();
			hsvColor.Value = 1.0f;
			GlowColor = hsvColor.ToColor();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Velocity = Vector3.Zero;

			Camera = new LookAtCamera();
			(Camera as LookAtCamera).TargetEntity = LastAttacker;
			(Camera as LookAtCamera).Origin = EyePos;
			(Camera as LookAtCamera).Rot = EyeRot;
			(Camera as LookAtCamera).TargetOffset = Vector3.Up * 64f;
			
			Inventory.DeleteContents();

			EnableDrawing = false;
			EnableAllCollisions = false;
			
			_ = Particles.Create( "particles/gib_blood.vpcf", Position + (Vector3.Up * (8)) );
			
			ShakeScreen( To.Everyone, Position );
			
			Sound.FromWorld( "gibbing", Position );
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
			Log.Trace( $"{Local.DisplayName} damaged another player" );
			
			PlaySound( "kill" );
			
			Hitmarker.CurrentHitmarker.OnHit();
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );
			lastDamageInfo = info;

			if ( info.Attacker is Player attacker )
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

			var panelTransform = new Sandbox.UI.PanelTransform();
			panelTransform.AddRotation( 0, 0, lean * -0.2f ); 

			var zOffset = (lastCameraPos - setup.Position).z * 4f;
			zOffset = zOffset.Clamp( -16f, 16f );
			zOffset = lastHudOffset.LerpTo( zOffset, 25.0f * Time.Delta );

			panelTransform.AddTranslateY( zOffset );

			lastHudOffset = zOffset;

			if ( InstagibHud.TiltingHudPanel != null )
			{
				InstagibHud.TiltingHudPanel.Style.Transform = panelTransform;
				InstagibHud.TiltingHudPanel.Style.Dirty();
			}

			lastCameraPos = setup.Position;
		}

	}
}
