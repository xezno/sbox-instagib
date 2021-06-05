﻿using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	/*
	<div class="frag">
		<text>YOU FRAGGED </text>
		<text class="highlight">Austin Powers</text>
	</div>
	 */
	
	public class FragMessage : Panel
	{
		public FragMessage( string target = "Austin Powers" )
		{
			SetClass( "frag", true );
			AddChild<Label>( Rand.Int(0, 100) == 1 ? "YOU SHAGGED " : "YOU FRAGGED " );
			AddChild<Label>( target );
		}
	}
}
