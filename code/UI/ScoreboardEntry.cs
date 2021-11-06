using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI
{
	public partial class ScoreboardEntry : Panel
	{
		public Client Client;

		private Label playerName;
		private Label kills;
		private Label deaths;
		private Label ratio;
		private Label ping;

		private Label vip;

		private Image avatar;

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			avatar = Add.Image( null, "avatar" );
			vip = Add.Label( "👑", "vip" );
			playerName = Add.Label( "PlayerName", "name" );

			kills = Add.Label( "k", "kills" );
			deaths = Add.Label( "d", "deaths" );
			ratio = Add.Label( "r", "ratio" );
			ping = Add.Label( "ping", "ping" );
		}

		RealTimeSince TimeSinceUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			playerName.Text = Client.Name;

			//
			// Kills/Deaths
			//
			{
				var killVal = Client.GetInt( "kills", 0 );
				var deathVal = Client.GetInt( "deaths", 0 );

				kills.Text = killVal.ToString();
				deaths.Text = deathVal.ToString();

				var ratioVal = (killVal / (float)deathVal);
				if ( deathVal == 0 )
					ratioVal = killVal;

				ratio.Text = ratioVal.ToString( "N1" );
			}

			avatar.SetTexture( $"avatar:{Client.SteamId}" );
			ping.Text = Client.Ping.ToString();
			SetClass( "me", Client == Local.Client );
			vip.SetClass( "visible", Client.SteamId == InstagibGlobal.AlexSteamId );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
