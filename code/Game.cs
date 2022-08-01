global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;

namespace Instagib;

public partial class InstagibGame : Sandbox.Game
{
	[Net] public BaseGamemode Gamemode { get; set; }

	public InstagibGame()
	{
		if ( IsServer )
		{
			_ = new Hud();
			Gamemode = new InstagibGamemode();
		}
	}

	[ConCmd.Admin( "gib_set_gamemode" )]
	public static void SetGamemode( string gamemodeLibraryName )
	{
		var game = Current as InstagibGame;
		if ( game == null )
			return;

		game.Gamemode = TypeLibrary.Create<BaseGamemode>( gamemodeLibraryName );
		Log.Trace( $"Setting gamemode to {game.Gamemode}" );
	}

	public override void Simulate( Client cl )
	{
		Gamemode?.Simulate();
		base.Simulate( cl );
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		Gamemode.RespawnPlayer( pawn );
		client.Pawn = pawn;
	}

	[ConCmd.Admin( "gib_wipe_leaderboard" )]
	public static void WipeLeaderboard()
	{
		var leaderBoard = GameServices.Leaderboard.Query( Global.GameIdent, skip: 10 ).Result;
		foreach ( var entry in leaderBoard.Entries )
		{
			var result = GameServices.SubmitScore( entry.PlayerId, -entry.Rating ).Result;
			Log.Info( $" Reset score of {entry.DisplayName}" );
			Log.Info( result );
		}
	}

	public static async void UpdateLeaderboard( Client client, int delta )
	{
		Host.AssertServer();

		GameServices.RecordEvent( client, $"K/D Delta change {delta}", delta );

		var matchingScores = await GameServices.Leaderboard.Query( Global.GameIdent, client.PlayerId );
		var currentScore = matchingScores.Entries.Select( x => x.Rating ).FirstOrDefault( 0 );

		await GameServices.SubmitScore( client.PlayerId, currentScore + delta );
	}

	public override void RenderHud()
	{
		base.RenderHud();

		if ( Local.Pawn is not Player player )
			return;

		//
		// scale the screen using a matrix, so the scale math doesn't invade everywhere
		// (other than having to pass the new scale around)
		//

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using ( Render.Draw2D.MatrixScope( matrix ) )
		{
			player.RenderHud( screenSize );
		}
	}
}

