namespace Instagib;
public partial class KillFeedEntry : Panel
{
	public Label Left { get; internal set; }
	public Label Right { get; internal set; }
	public Label Method { get; internal set; }

	public RealTimeSince TimeSinceBorn = 0;

	public KillFeedEntry()
	{
		Left = Add.Label( "", "left" );
		Method = Add.Label( "", "method" );
		Right = Add.Label( "", "right" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( TimeSinceBorn > 5 )
		{
			Delete();
		}
	}
}
