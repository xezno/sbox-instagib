namespace Instagib;

partial class Player
{
	[ConCmd.Admin( "gib_godmode" )]
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
