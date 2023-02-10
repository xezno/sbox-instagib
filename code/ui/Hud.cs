namespace Instagib;

public class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.SetTemplate( "ui/Hud.html" );
	}
}
