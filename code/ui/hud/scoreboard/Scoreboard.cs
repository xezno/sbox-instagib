namespace Instagib;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

public partial class Scoreboard : Panel
{
	private RealTimeSince timeSinceSorted;

	public Panel Canvas { get; protected set; }
	Dictionary<Client, ScoreboardEntry> Rows = new();

	public Panel Header { get; protected set; }

	public Scoreboard()
	{
		StyleSheet.Load( "/ui/hud/scoreboard/Scoreboard.scss" );
		AddClass( "scoreboard" );

		AddControls();
		AddHeader();

		Canvas = Add.Panel( "canvas" );
	}
	protected void AddControls()
	{
		var controls = new Panel( this, "controls" );
		controls.Add.InputHint( InputButton.PrimaryAttack, "Fire" );
		controls.Add.InputHint( InputButton.SecondaryAttack, "Zoom" );
		controls.Add.InputHint( InputButton.Run, "Dash" );
		controls.Add.InputHint( InputButton.Jump, "Jump / Double Jump" );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "open", ShouldBeOpen() );

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

		if ( timeSinceSorted > 0.1f )
		{
			timeSinceSorted = 0;

			//
			// Sort by number of kills, then number of deaths
			//
			Canvas.SortChildren<ScoreboardEntry>( ( x ) => (-x.Client.GetInt( "kills" ) * 1000) + x.Client.GetInt( "deaths" ) );
		}
	}

	public virtual bool ShouldBeOpen()
	{
		return Input.Down( InputButton.Score );
	}


	protected virtual void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "Name", "name" );
		Header.Add.Label( "Kills", "kills" );
		Header.Add.Label( "Deaths", "deaths" );
		Header.Add.Label( "Ping", "ping" );
	}

	protected virtual ScoreboardEntry AddClient( Client entry )
	{
		var p = Canvas.AddChild<ScoreboardEntry>();
		p.Client = entry;
		return p;
	}
}
