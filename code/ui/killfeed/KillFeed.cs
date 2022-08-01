namespace Instagib;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/ui/killfeed/KillFeed.scss" );
	}

	[InstagibEvent.Game.Kill]
	public virtual Panel OnKill( KillData killData )
	{
		Log.Trace( $"KillFeed: {killData}" );

		var e = Current.AddChild<KillFeedEntry>();
		e.SetClass( "me", killData.AttackerPlayerID == (Local.Client?.PlayerId) || killData.VictimPlayerID == (Local.Client?.PlayerId) );

		e.Left.Text = killData.AttackerName;
		e.Method.Text = killData.Method;
		e.Right.Text = killData.VictimName;

		return e;
	}
}
