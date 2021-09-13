using System;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class GrappleIndicator : Panel
	{
		private static GrappleIndicator Instance { get; set; }

		private static float lastLeft = 0;
		private static float lastTop = 0;

		public enum State
		{
			Default,
			Cooldown,
			Active
		}

		public GrappleIndicator()
		{
			Instance = this;
			StyleSheet.Parse( $"grappleindicator {{ width: 4px; aspect-ratio: 1; }}" );
		}

		public static void SetCanGrapple( State state )
		{
			switch ( state )
			{
				case State.Default:
					Instance.RemoveClass( "Cooldown" );
					Instance.RemoveClass( "Active" );
					Instance.AddClass( "Default" );
					break;
				case State.Cooldown:
					Instance.RemoveClass( "Default" );
					Instance.RemoveClass( "Active" );
					Instance.AddClass( "Cooldown" );
					break;
				case State.Active:
					Instance.RemoveClass( "Default" );
					Instance.RemoveClass( "Cooldown" );
					Instance.AddClass( "Active" );
					break;
			}
		}

		public static void MoveTo( Vector3 point, bool lerp = true )
		{
			var screenPointVec3 = point.ToScreen();
			var screenPoint = new Vector2( screenPointVec3.x, screenPointVec3.y) * Screen.Size * Instance.ScaleFromScreen;

			float newLeft, newTop;
			if ( lerp )
			{
				newLeft = lastLeft.LerpTo( screenPoint.x, Time.Delta * 33 );
				newTop = lastTop.LerpTo( screenPoint.y, Time.Delta * 33 );
			}
			else
			{
				newLeft = screenPoint.x;
				newTop = screenPoint.y;
			}
			
			Instance.Style.Left = newLeft;
			Instance.Style.Top = newTop;
			Instance.Style.Dirty();
			
			lastLeft = newLeft;
			lastTop = newTop;
		}

		public static void MoveToCenter()
		{
			Instance.Style.Left = Length.Percent( 50 );
			Instance.Style.Top = Length.Percent( 50 );
			lastLeft = 0.5f * Screen.Size.x + 64;
			lastTop = 0.5f * Screen.Size.y + 64;
			Instance.Style.Dirty();
		}
	}
}
