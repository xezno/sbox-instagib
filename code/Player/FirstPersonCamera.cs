using Sandbox;

namespace Instagib
{
	public class FirstPersonCamera : Camera
	{
		Vector3 lastPos;

		private float lastFov;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePos;
			Rotation = pawn.EyeRot;

			lastPos = Position;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;
			if ( eyePos.Distance( lastPos ) < 250 )
			{
				Position = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			}
			else
			{
				Position = eyePos;
			}

			Rotation = pawn.EyeRot;

			FieldOfView = PlayerSettings.Fov;
			if ( pawn.ActiveChild is Railgun { IsZooming: true } )
			{
				FieldOfView = PlayerSettings.ZoomedFov;
			}

			FieldOfView = lastFov.LerpTo( FieldOfView, 20 * Time.Delta );
			lastFov = FieldOfView;

			ZNear = 3;
			ZFar = 20000;

			Viewer = pawn;
			lastPos = Position;
		}
	}
}
