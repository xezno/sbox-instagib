using Sandbox;

namespace Instagib
{
	public class FirstPersonCamera : Camera
	{
		Vector3 lastPos;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			lastPos = Pos;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;
			if ( eyePos.Distance( lastPos ) < 300 ) // TODO: Tweak this, or add a way to invalidate lastpos when teleporting
			{
				Pos = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			}
			else
			{
				Pos = eyePos;
			}

			Rot = pawn.EyeRot;

			FieldOfView = 80;

			Viewer = pawn;
			lastPos = Pos;
		}
	}
}
