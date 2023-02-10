﻿namespace Instagib;

partial class QuakeWalkController
{
	private int line = 0;

	private void LogToScreen( string text )
	{
		if ( !Debug )
			return;

		string realm = Game.IsClient ? "CL" : "SV";
		float starty = Game.IsClient ? 150 : 250;

		var pos = new Vector2( 760, starty + (line++ * 16) );
		DebugOverlay.ScreenText( $"{realm}: {text}", pos );
	}
}
