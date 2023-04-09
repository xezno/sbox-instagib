global using Editor;
global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Utility;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

namespace Instagib;

public partial class InstagibGame : Sandbox.GameManager
{
	[ConVar.Replicated( "gib_grapples_enabled" )]
	public static bool GrapplesEnabled { get; set; }

	[Net] public BaseGamemode Gamemode { get; set; }

	public InstagibGame()
	{
		if ( Game.IsServer )
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

	public override void Simulate( IClient cl )
	{
		Gamemode?.Simulate();
		base.Simulate( cl );
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player( client );
		Gamemode.RespawnPlayer( pawn );
		client.Pawn = pawn;
	}

	public override void RenderHud()
	{
		base.RenderHud();

		if ( Game.LocalPawn is not Player player )
			return;

		player.RenderHud();
	}
}

