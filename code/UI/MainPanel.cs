﻿using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		private float PlayerSpeedMph => Local.Client.Pawn.Velocity.WithZ( 0 ).Length // in/s
		                                * 0.0254f // m/s
		                                * 2.23694f; // mph
		
		public string PlayerHealth => $"{Local.Client.Pawn.Health.CeilToInt()}";
		public string PlayerSpeed => $"{Local.Client.Pawn.Velocity.WithZ( 0 ).Length:N0}u/s (top: {topSpeed}u/s)";

		private int topSpeed = 0;

		public MainPanel()
		{
			SetTemplate( "/Code/UI/InstagibHud.html" );
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" ); // Loading in HTML doesn't work for whatever reason
			SetClass( "mainpanel", true );
		}

		public override void Tick()
		{
			base.Tick();
			var plySpeed = Local.Client.Pawn.Velocity.WithZ( 0 ).Length.CeilToInt();
			if ( plySpeed > topSpeed )
				topSpeed = plySpeed;
		}
	}
}
