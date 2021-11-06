using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		public Crosshair()
		{
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is Player { ActiveChild: Railgun railgun })
			{
				SetClass( "canfire", railgun.TimeSincePrimaryAttack > 1/railgun.PrimaryRate );
			}
		}
	}
}
