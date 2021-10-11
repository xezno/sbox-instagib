using Sandbox;

namespace Instagib
{
	public partial class GrappleHookEntity : ModelEntity
	{
		public Vector3 Target { get; set; }
		public float HookSpeed { get; set; }

		public bool Attached { get; private set; }

		private Vector3 initialPos;
		private float t;

		public override void Spawn()
		{
			base.Spawn();

			initialPos = Position;
			Transmit = TransmitType.Always;
		}

		public override void Simulate( Client cl )
		{
			if ( !Attached )
			{
				Position = initialPos.LerpTo( Target, t );
				t += Time.Delta * 8;

				if ( t >= 1 )
				{
					Attached = true;
					PlaySound( "grapple" );
				}
			}
		}
	}
}
