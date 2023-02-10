namespace Instagib;

public class DashIndicators : Panel
{
	private List<Panel> dashPanels = new();

	public DashIndicators()
	{
		AddClass( "dash-indicators" );
		StyleSheet.Load( "/ui/hud/DashIndicators.scss" );

		for ( int i = 0; i < QuakeWalkController.DashCount; ++i )
		{
			CreateDashPanel( i );
		}

		BindClass( "visible", () => InstagibGame.SelectedMoveSet == InstagibGame.MoveSet.Modern );
	}

	private void CreateDashPanel( int index )
	{
		var dashPanel = new Panel( this, "dash-indicator" );
		dashPanels.Add( dashPanel );

		dashPanel.BindClass( "active", () =>
		{
			if ( Game.LocalPawn is not Player { Controller: QuakeWalkController quakeWalkController } )
				return false;

			return quakeWalkController.DashesLeft > index;
		} );
	}
}
