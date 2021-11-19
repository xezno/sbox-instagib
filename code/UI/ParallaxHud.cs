using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class ParallaxHud : Panel
	{
		private float PlayerSpeed { get; set; }
		public string PlayerSpeedText => $"{PlayerSpeed:N0}u/s";

		public ParallaxHud()
		{
			SetTemplate( "/Code/UI/ParallaxHud.html" );
			StyleSheet.Load( "/Code/UI/ParallaxHud.scss" ); // Loading in HTML doesn't work for whatever reason
		}

		public override void Tick()
		{
			base.Tick();
			PlayerSpeed = Local.Pawn.Velocity.Cross( Vector3.Up ).Length;
		}
	}
}
