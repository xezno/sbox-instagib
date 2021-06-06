﻿using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		private float PlayerSpeedMph => Local.Client.Pawn.Velocity.WithZ( 0 ).Length // in/s
		                                * 0.0254f // m/s
		                                * 2.23694f; // mph
		
		public string PlayerHealth => $"{Local.Client.Pawn.Health}";
		public string PlayerSpeed => $"{Local.Client.Pawn.Velocity.WithZ( 0 ).Length:N0}u/s ({PlayerSpeedMph:N0}mph)";

		public MainPanel()
		{
			SetTemplate( "/UI/InstagibHud.html" );
			SetClass( "mainpanel", true );
		}
	}
}
