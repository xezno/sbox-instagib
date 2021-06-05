using Sandbox;

namespace Instagib
{
	public class ViewModel : BaseViewModel
	{
		[ClientVar( "viewmodel_centered" )] public static bool Centered { get; set; } = true;

		protected float SwingInfluence => 0.005f;
		protected float ReturnSpeed => 5.0f;
		protected float MaxOffsetLength => 10.0f;
		protected float BobCycleTime => 7;
		protected Vector3 BobDirection => new Vector3( 0.0f, 0.1f, 0.2f );

		private Vector3 swingOffset;
		private float lastPitch;
		private float lastYaw;
		private float bobAnim;

		private bool activated = false;

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( !Local.Pawn.IsValid() )
				return;

			if ( !activated )
			{
				lastPitch = camSetup.Rotation.Pitch();
				lastYaw = camSetup.Rotation.Yaw();

				activated = true;
			}

			Position = camSetup.Position;
			Rotation = camSetup.Rotation;

			camSetup.ViewModel.FieldOfView = 55;

			var newPitch = Rotation.Pitch();
			var newYaw = Rotation.Yaw();

			var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
			var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

			var playerVelocity = Local.Pawn.Velocity;
			var verticalDelta = playerVelocity.z * Time.Delta;
			var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
			verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
			pitchDelta -= verticalDelta * 1;

			var offset = CalcSwingOffset( pitchDelta, yawDelta );
			offset += CalcBobbingOffset( playerVelocity );

			if ( Centered )
			{
				offset -= new Vector3( 0f, -2.5f, 0.75f );
			}

			Position += Rotation * offset;

			lastPitch = newPitch;
			lastYaw = newYaw;
		}

		protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
		{
			Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

			swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
			swingOffset += (swingVelocity * SwingInfluence);

			if ( swingOffset.Length > MaxOffsetLength )
			{
				swingOffset = swingOffset.Normal * MaxOffsetLength;
			}

			return swingOffset;
		}

		protected Vector3 CalcBobbingOffset( Vector3 velocity )
		{
			if ( Owner.GroundEntity != null )
			{
				bobAnim += Time.Delta * BobCycleTime;
			}
			else
			{
				if ( bobAnim > System.MathF.PI / 2f + 0.01f )
					bobAnim -= Time.Delta * BobCycleTime * 0.1f;
				else if ( bobAnim < System.MathF.PI / 2f + 0.01f )
					bobAnim += Time.Delta * BobCycleTime * 0.1f;
			}

			var twoPI = System.MathF.PI * 2.0f;

			if ( bobAnim > twoPI )
			{
				bobAnim -= twoPI;
			}

			var speed = new Vector2( velocity.x, velocity.y ).Length;
			speed = speed > 10.0 ? speed : 0.0f;
			var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
			offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

			return offset;
		}
	}
}
