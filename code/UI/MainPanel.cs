﻿using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		public Label MenuPrompt { get; set; }
		public string MenuPromptText => $"Press {Input.GetKeyWithBinding( "iv_menu" ).ToUpper()} to open the menu";
		private float PlayerSpeed { get; set; }
		private float PlayerSpeedMph => PlayerSpeed // in/s
						* 0.0254f // m/s
		                                * 2.23694f; // mph
		
		public string PlayerHealthText => $"{Local.Client.Pawn.Health.CeilToInt()}";
		public string PlayerSpeedText => $"{PlayerSpeed:N0}u/s (top: {topSpeed}u/s)";

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

			PlayerSpeed = Local.Pawn.Velocity.Cross( Vector3.Up ).Length;

			MenuPrompt.SetClass( "visible", PlayerSpeed < 30f );

			if ( PlayerSpeed.CeilToInt() > topSpeed )
				topSpeed = PlayerSpeed.CeilToInt();
		}
	}
}
