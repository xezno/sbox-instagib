﻿using System.Runtime.ExceptionServices;
using Sandbox;
using Sandbox.Rcon;
using Sandbox.UI;

namespace Instagib.UI
{
	public class Crosshair : Label
	{
		private static Crosshair Instance { get; set; }
		
		private Panel up;
		private Panel left;
		private Panel right;
		private Panel down;
		
		public Crosshair()
		{
			Instance = this;
			
			SetClass( "crosshair", true );
			SetText( "a" );

			var crosshairMove = AddChild<Panel>( "crosshair-move" );
			
			up = crosshairMove.AddChild<Panel>( "up" );
			left = crosshairMove.AddChild<Panel>( "left" );
			right = crosshairMove.AddChild<Panel>( "right" );
			down = crosshairMove.AddChild<Panel>( "down" );
			
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
		}

		[Event.BuildInput]
		public void BuildInput( InputBuilder input )
		{
			Host.AssertClient();

			up.SetClass( "active", input.Down( InputButton.Forward ) );
			left.SetClass( "active", input.Down( InputButton.Left ) );
			right.SetClass( "active", input.Down( InputButton.Right ) );
			down.SetClass( "active", input.Down( InputButton.Back ) );
		}

		[ClientCmd( "crosshair_glyph" )]
		public static void SetCrosshair( string character )
		{
			if ( character.Length != 1 )
				return;
			
			Instance.SetText( character );
		}
	}
}
