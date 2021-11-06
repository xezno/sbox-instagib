using System;
using Sandbox;

namespace Instagib
{
	public partial class LookAtCamera : Camera
	{
		/// <summary>
		/// Origin of the camera
		/// </summary>
		[Net, Predicted] public Vector3 Origin { get; set; }

		/// <summary>
		/// Entity to look at
		/// </summary>
		[Net, Predicted] public Entity TargetEntity { get; set; }

		/// <summary>
		/// Offset from the entity to look at
		/// </summary>
		[Net, Predicted] public Vector3 TargetOffset { get; set; } 

		/// <summary>
		/// Min fov when target is max distance away from origin
		/// </summary>
		public float MinFov { get; set; } = 50;

		/// <summary>
		/// Max fov when target is near the origin
		/// </summary>
		public float MaxFov { get; set; } = 100;

		/// <summary>
		/// How far away to reach min fov
		/// </summary>
		public float MinFovDistance { get; set; } = 1000;

		/// <summary>
		/// How quick to lerp to target
		/// </summary>
		public float LerpSpeed { get; set; } = 4.0f;

		private Vector3 lastTargetPos;

		protected virtual Vector3 GetTargetPos()
		{
			return TargetEntity.IsValid() ? ( TargetEntity.Position + TargetOffset ) : Vector3.Zero;
		}

		public override void Activated()
		{
			lastTargetPos = GetTargetPos();
		}

		public override void Update()
		{
			Viewer = null;
			Position = Origin;

			var targetPos = GetTargetPos();

			if ( LerpSpeed > 0.0f )
			{
				targetPos = lastTargetPos.LerpTo( targetPos, Time.Delta * LerpSpeed );
			}

			lastTargetPos = targetPos;

			var targetDelta = ( targetPos - Origin );
			var targetDistance = targetDelta.Length;
			var targetDirection = targetDelta.Normal;

			// should be a helper func
			Rotation = Rotation.From( new Angles(
				((float)Math.Asin( targetDirection.z )).RadianToDegree() * -1.0f,
				((float)Math.Atan2( targetDirection.y, targetDirection.x )).RadianToDegree(),
				0.0f ) );

			var fovDelta = MinFovDistance > 0.0f ? ( targetDistance / MinFovDistance ) : 0.0f;
			FieldOfView = MaxFov.LerpTo( MinFov, fovDelta );
		}
	}
}
