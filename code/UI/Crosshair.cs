using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		Panel[] elements;
		public Crosshair()
		{
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			elements = new Panel[5];
			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
				elements[i] = p;
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is Player { ActiveChild: Railgun railgun } )
			{
				SetClass( "canfire", railgun.TimeSincePrimaryAttack > 1 / railgun.PrimaryRate );
			}

			foreach ( var element in elements )
			{
				element.Style.BackgroundColor = PlayerSettings.CrosshairColor;
			}
		}
	}
}
