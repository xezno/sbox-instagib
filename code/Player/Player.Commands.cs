namespace OpenArena;

partial class Player
{
	[ConCmd.Admin( "oa_godmode" )]
	public static void GodMode()
	{
		var caller = ConsoleSystem.Caller;
		var pawn = caller.Pawn;

		if ( pawn is Player player )
		{
			player.IsInvincible = !player.IsInvincible;
			Log.Trace( $"Godmode is now {(player.IsInvincible ? "ON" : "OFF")}" );
		}
	}
}
