using Sandbox;

namespace Instagib
{
	public class ViewModel : BaseViewModel
	{
		[ClientVar( "viewmodel_centered" )] public static bool Centered { get; set; } = false;
		[ClientVar( "viewmodel_visible" )] public static bool Visible { get; set; } = true;

		protected float SwingInfluence => 0.025f;
		protected float ReturnSpeed => 5.0f;
		protected float MaxOffsetLength => 10.0f;
		protected float BobCycleTime => 14;
		protected Vector3 BobDirection => new( 0.0f, 0.1f, 0.2f );

		private Vector3 swingOffset;
		private float lastPitch;
		private float lastYaw;
		private float bobAnim;

		private bool activated;

		private Vector3 CenteredOffset => new( -25f, 0f, 10f );
		private Vector3 RightHandOffset => new( -25f, 10f, 10f );

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( !Visible )
			{
				EnableDrawing = false;
			}
			else
			{
				EnableDrawing = true;
			}

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

			camSetup.ViewModel.FieldOfView = 45;

			var newPitch = Rotation.Pitch();
			var newYaw = Rotation.Yaw();

			var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
			var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

			var playerVelocity = Local.Pawn.Velocity;
			var verticalDelta = playerVelocity.Z * Time.Delta;
			var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
			verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).Y ));
			pitchDelta -= verticalDelta * 1;

			var offset = CalcSwingOffset( pitchDelta, yawDelta );
			offset += CalcBobbingOffset( playerVelocity );

			if ( Centered )
			{
				offset -= CenteredOffset;
			}
			else
			{
				offset -= RightHandOffset;
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
			var halfPI = System.MathF.PI * 0.5f;
			var twoPI = System.MathF.PI * 2.0f;
			
			if ( Owner.GroundEntity != null )
			{
				bobAnim += Time.Delta * BobCycleTime;
			}
			else
			{
				// In air - return to center
				if ( bobAnim > halfPI + 0.1f )
					bobAnim -= Time.Delta * BobCycleTime * 0.05f;
				else if ( bobAnim < halfPI + 0.1f )
					bobAnim += Time.Delta * BobCycleTime * 0.05f;
				else
					bobAnim = halfPI;
			}

			if ( bobAnim > twoPI )
			{
				bobAnim -= twoPI;
			}

			var speed = new Vector2( velocity.X, velocity.Y ).Length;
			speed = speed > 10.0 ? speed : 0.0f;
			var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
			offset = offset.WithZ( -System.MathF.Abs( offset.Z ) );

			return offset;
		}
	}
}
