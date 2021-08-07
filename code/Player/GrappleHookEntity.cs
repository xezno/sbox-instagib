using Sandbox;

namespace Instagib
{
	public partial class GrappleHookEntity : ModelEntity
	{
		public Vector3 Target { get; set; }
		public float HookSpeed { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			Position = Target;
		}
	}
}
