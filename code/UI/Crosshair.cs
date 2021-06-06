using System.Runtime.ExceptionServices;
using Sandbox;
using Sandbox.Rcon;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Panel
	{
		private Panel up;
		private Panel left;
		private Panel right;
		private Panel down;
		
		public Crosshair()
		{
			SetClass( "crosshair", true );
			AddChild<Panel>().SetClass( "crosshair-inner", true );

			var crosshairMove = AddChild<Panel>( "crosshair-move" );
			
			up = crosshairMove.AddChild<Panel>( "up" );
			left = crosshairMove.AddChild<Panel>( "left" );
			right = crosshairMove.AddChild<Panel>( "right" );
			down = crosshairMove.AddChild<Panel>( "down" );
			
			StyleSheet.Load( "/UI/InstagibHud.scss" );
		}

		[Event.BuildInput]
		public void BuildInput( InputBuilder input )
		{
			Host.AssertClient();

			up.SetClass( "active", input.Down( InputButton.Forward ) );
			left.SetClass( "active", input.Down( InputButton.Left ) );
			right.SetClass( "active", input.Down( InputButton.Right ) );
			down.SetClass( "active", input.Down( InputButton.Back ) );
		}
	}
}
