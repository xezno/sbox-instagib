using Sandbox;

namespace Instagib
{
	public partial class GrappleHookEntity : ModelEntity
	{
		public Vector3 Target { get; set; }
		public float HookSpeed { get; set; }

		private Vector3 initialPosition;
		private float positionT;

		public override void Spawn()
		{
			base.Spawn();
			initialPosition = Position;
			Position = Target;
		}

		//[Event.Tick.Server]
		//public void OnTick()
		//{
		//	positionT += HookSpeed * Time.Delta;
		//	Position = initialPosition.LerpTo( Target, positionT );
		//}
	}
}
