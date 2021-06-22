using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Instagib.UI
{
	public partial class ScoreboardEntry : Panel
	{
		private PlayerScore.Entry entry;
		
		private Label playerName;
		private Label kills;
		private Label deaths;
		private Label ratio;

		private Label steamId;

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			playerName = Add.Label( "PlayerName", "name" );
			kills = Add.Label( "k", "kills" );
			deaths = Add.Label( "d", "deaths" );
			ratio = Add.Label( "r", "ratio" );
			steamId = Add.Label( "steamid", "steamid" );
		}

		public virtual void UpdateFrom( PlayerScore.Entry entry )
		{
			this.entry = entry;

			playerName.Text = entry.GetString( "name" );

			var killVal = entry.Get<int>( "kills", 0 );
			var deathVal = entry.Get<int>( "deaths", 0 );
			
			kills.Text = killVal.ToString();
			deaths.Text = deathVal.ToString();

			var ratioVal = (killVal / (float)deathVal);
			if ( deathVal == 0 )
				ratioVal = killVal;
			
			ratio.Text = ratioVal.ToString("N1");
			steamId.Text = (entry.Get<ulong>( "steamid", 0 ) == Local.Client.SteamId).ToString();

			SetClass( "me", Local.Client != null && entry.Get<ulong>( "steamid", 0 ) == Local.Client.SteamId );
		}
	}
}
