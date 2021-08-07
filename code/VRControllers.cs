using Sandbox;

namespace Instagib
{
	public class VRHead : ModelEntity
	{
		private Transform OffsetTransform => new Transform( Vector3.Zero, Rotation.From( 90, 180, 0 ) );
		
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/citizen_props/hotdog01.vmdl" );
			
			Log.Info( "VR Controller Right Spawned" );
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );
			if ( !VR.Enabled )
				return;

			Position = cl.Pawn.EyePos + cl.Pawn.EyeRot.Forward * 64;
			Rotation = cl.Pawn.EyeRot;
		}
	}
	
	public class VRControllerRight : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();
			// SetModel( "weapons/railgun/models/wpn_qc_railgun.vmdl" );
			Log.Info( "VR Controller Right Spawned" );
		}

		// [Event.Frame]
		// public void OnClientTick()
		// {
		// 	if ( !VR.Enabled )
		// 		return;
		//
		// }
	}
	
	public class VRControllerLeft : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/hands/sm_lefthand.vmdl" );
			
			Log.Info( "VR Controller Left Spawned" );
		}

		[Event.Frame]
		public void OnClientTick()
		{
			if ( !VR.Enabled )
				return;
			
			Transform = Input.VR.LeftHand.Transform;
			DebugOverlay.Text( Transform.Position, $"Left Controller Joy:{Input.VR.LeftHand.Joystick.Value} Trig:{Input.VR.LeftHand.Trigger.Value} Grip:{Input.VR.LeftHand.Grip.Value}" );
		}
	}
}
