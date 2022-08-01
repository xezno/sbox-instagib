namespace Instagib;

public class Leaderboard : Panel
{
	private LeaderboardEntry leader;
	private LeaderboardEntry local;

	public Leaderboard()
	{
		AddClass( "leaderboard" );

		StyleSheet.Load( "/ui/hud/Leaderboard.scss" );

		leader = new( this );
		local = new( this );
	}

	public override void Tick()
	{
		base.Tick();

		var orderedClients = ClientExtensions.OrderClients();
		var leaderClient = orderedClients.FirstOrDefault();

		leader.SetPlayer( leaderClient );

		if ( leaderClient == Local.Client )
			local.SetPlayer( orderedClients.Skip( 1 ).FirstOrDefault() );
		else
			local.SetPlayer( Local.Client );
	}
}

class LeaderboardEntry : Panel
{
	private Label place;
	private Image avatar;
	private Label name;
	private Label score;

	public LeaderboardEntry( Panel parent )
	{
		AddClass( "leaderboard-entry" );

		place = Add.Label( "0", "leaderboard-entry-place" );
		avatar = Add.Image( "avatar:0", "leaderboard-entry-avatar" );
		name = Add.Label( "Player", "leaderboard-entry-name" );
		score = Add.Label( "0", "leaderboard-entry-score" );

		Parent = parent;
	}

	public void SetPlayer( Client client )
	{
		if ( client == null )
		{
			Style.Display = DisplayMode.None;
			return;
		}

		Style.Display = DisplayMode.Flex;

		place.Text = $"{client.GetPlacement()}";
		avatar.SetTexture( $"avatar:{client.PlayerId}" );
		name.Text = client.Name;
		score.Text = $"{client.GetInt( "kills" )}";
	}
}
