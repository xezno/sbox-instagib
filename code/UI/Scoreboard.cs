using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Instagib.UI
{
	public partial class Scoreboard<T> : Panel where T : ScoreboardEntry, new()
	{
		public Panel Canvas { get; protected set; }
		public Panel EntryContainer { get; protected set; }
		Dictionary<Client, T> Rows = new();

		public Panel Header { get; protected set; }

		public Scoreboard()
		{
			StyleSheet.Load( "/Code/UI/Scoreboard.scss" );
			AddClass( "scoreboard" );

			Canvas = Add.Panel( "canvas" );
			Canvas.Add.Label( "SCOREBOARD", "title" );

			AddHeader();

			EntryContainer = Canvas.Add.Panel( "entry-container" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );
			if ( !IsVisible )
				return;

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

			Sort();
		}

		protected virtual void AddHeader()
		{
			Header = Canvas.Add.Panel( "header" );
			Header.Add.Label( "Name", "name" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "Deaths", "deaths" );
			Header.Add.Label( "Ratio", "ratio" );
			Header.Add.Label( "Ping", "ping" );
		}

		private void Sort()
		{
			Canvas.SortChildren( ( panel1, panel2 ) =>
			{
				if ( panel1 is ScoreboardEntry a && panel2 is ScoreboardEntry b )
				{
					return InstagibGlobal.SortClients( a.Client, b.Client );
				}
				return 1;
			} );
		}

		protected virtual T AddClient( Client entry )
		{
			var p = Canvas.AddChild<T>();
			p.Client = entry;
			return p;
		}
	}
}
