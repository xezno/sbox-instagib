using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Instagib.UI
{
	public partial class Scoreboard<T> : Panel where T : ScoreboardEntry, new()
	{
		public Panel Canvas { get; protected set; }
		public Panel EntryContainer { get; protected set; }
		Dictionary<int, T> Entries = new ();

		public Panel Header { get; protected set; }

		public Scoreboard()
		{
			StyleSheet.Load( "/Code/UI/Scoreboard.scss" );
			AddClass( "scoreboard" );

			Canvas = Add.Panel( "canvas" );
			Canvas.Add.Label( "SCORE", "title" );

			AddHeader();
			
			EntryContainer = Canvas.Add.Panel( "entry-container" );

			PlayerScore.OnPlayerAdded += AddPlayer;
			PlayerScore.OnPlayerUpdated += UpdatePlayer;
			PlayerScore.OnPlayerRemoved += RemovePlayer;

			foreach ( var player in PlayerScore.All )
			{
				AddPlayer( player );
			}
			
			Sort();
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );
		}

		protected virtual void AddHeader() 
		{
			Header = Canvas.Add.Panel( "header" );
			Header.Add.Label( "Name", "name" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "Deaths", "deaths" );
			Header.Add.Label( "Ratio", "ratio" );
			Header.Add.Label( "Ping", "ping" );
			Header.Add.Label( "Hit %", "accuracy" );
		}

		private void Sort()
		{			
			EntryContainer.SortChildren( ( panel1, panel2 ) =>
			{
				if ( panel1 is ScoreboardEntry a && panel2 is ScoreboardEntry b )
				{
					var aKills = a.Entry.Get( "kills", 0 );
					var bKills = b.Entry.Get( "kills", 0 );

					if ( bKills > aKills )
						return 1;
					if ( aKills > bKills )
						return -1;

					return 0;
				}

				return 1;
			} );
		}

		protected virtual void AddPlayer( PlayerScore.Entry entry )
		{
			var p = EntryContainer.AddChild<T>();
			p.UpdateFrom( entry );

			Entries[entry.Id] = p;

			Sort();
		}

		protected virtual void UpdatePlayer( PlayerScore.Entry entry )
		{
			if ( Entries.TryGetValue( entry.Id, out var panel ) )
			{
				panel.UpdateFrom( entry );
			}
			
			Sort();
		}

		protected virtual void RemovePlayer( PlayerScore.Entry entry )
		{
			if ( Entries.TryGetValue( entry.Id, out var panel ) )
			{
				panel.Delete();
				Entries.Remove( entry.Id );
			}
			
			Sort();
		}
	}
}
