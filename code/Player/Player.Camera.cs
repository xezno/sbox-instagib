using System;
using Instagib.UI;
using Sandbox;

namespace Instagib
{
	public partial class Player
	{
		//
		// Dynamic hud / camera
		//
		private Vector3 lastCameraPos = Vector3.Zero;
		private Rotation lastCameraRot = Rotation.Identity;
		private float lastHudOffset;

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
			setup.Rotation *= Rotation.From( 0, 0, appliedLean ) * PlayerSettings.ViewTiltMultiplier;

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
