namespace Instagib;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{
	private RealTimeSince timeSinceSorted;

	protected override void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "Name", "name" );
		Header.Add.Label( "Kills", "kills" );
		Header.Add.Label( "Deaths", "deaths" );
		Header.Add.Label( "Ping", "ping" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible ) return;

		if ( timeSinceSorted > 0.1f )
		{
			timeSinceSorted = 0;

			//
			// Sort by number of kills, then number of deaths
			//
			Canvas.SortChildren<ScoreboardEntry>( ( x ) => (-x.Client.GetInt( "kills" ) * 1000) + x.Client.GetInt( "deaths" ) );
		}
	}
}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{

}
