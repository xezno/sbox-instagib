using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Instagib.UI.Elements
{
	public class Leaderboard : Panel
	{
		public Panel Canvas { get; protected set; }
		public Panel EntryContainer { get; protected set; }
		Dictionary<Client, LeaderboardEntry> Rows = new();

		public Panel Header { get; protected set; }

		public Leaderboard()
		{
			Canvas = Add.Panel( "canvas" );
			EntryContainer = Canvas.Add.Panel( "entry-container" );
		}

		public override void Tick()
		{
			base.Tick();

			//
			// Clients that were added
			//
			foreach ( var client in Client.All.Except( Rows.Keys ) )
			{
				var entry = AddClient( client );
				Rows[client] = entry;
			}

			foreach ( var client in Rows.Keys.Except( Client.All ) )
			{
				if ( Rows.TryGetValue( client, out var row ) )
				{
					row?.Delete();
					Rows.Remove( client );
				}
			}
		}

		protected virtual LeaderboardEntry AddClient( Client entry )
		{
			var p = Canvas.AddChild<LeaderboardEntry>();
			p.Client = entry;
			return p;
		}
	}

	public class LeaderboardEntry : Panel
	{
		public Label playerName;
		public Label kills;

		public Client Client;

		public LeaderboardEntry()
		{
			playerName = Add.Label( "Player Name", "player-name" );
			kills = Add.Label( "0", "kills" );
		}

		public virtual void UpdateData()
		{
			playerName.Text = Client.Name;
			var killVal = Client.GetInt( "kills", 0 );
			kills.Text = killVal.ToString();

			var sortedClients = new List<Client>( Client.All );
			sortedClients.Sort( InstagibGlobal.SortClients );

			int place = -1;
			//
			// Local player position
			//
			for ( int i = 0; i < sortedClients.Count; i++ )
			{
				Client client = sortedClients[i];

				if ( client == Client )
				{
					place = i;
					break;
				}
			}

			Log.Trace( $"place: {place}" );

			SetClass( "visible", place < 3 );
		}

		TimeSince TimeSinceUpdate;

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

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
