using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instagib.UI.PostGameScreens
{
	public class WinnerScreen : BasePostGameScreen
	{
		public WinnerScreen() : base()
		{
			StyleSheet.Load( "/Code/UI/PostGameScreens/WinnerScreen.scss" );

			Game.Instance.GameType.CreateWinnerElements( this );
		}
	}

	public class WinnerText : Panel
	{
		public WinnerText( string position, Client player, string className )
		{
			Add.Label( position, "position " + className );
			Add.Label( player.Name, "player " + className );

			var kdrString = $"K: {player.GetInt( "kills" )} | D: {player.GetInt( "deaths" )}";
			Add.Label( kdrString, "stats " + className );
		}
	}

	public class WinnerParticle : Label
	{
		public Vector2 Origin { get; set; }
		private Vector2 Position { get; set; }
		private Vector2 Velocity { get; set; }

		public WinnerParticle()
		{
			var emojis = new string[] { "🥳", "🎉", "🎊", "🎈", "🤩", "👏", "🥂", "👍" };

			SetText( Rand.FromArray( emojis ) );

			_ = TransitionOut();

			//
			// Velocity
			//
			var rand = (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.25f;
			Velocity = (rand.Normal * 500).WithY( 0 );

			//
			// Rotation
			//
			var transform = new PanelTransform();
			transform.AddRotation( 0, 0, Rand.Float( -45, 45 ) );
			Style.Transform = transform;
		}

		public override void Tick()
		{
			base.Tick();
			var screenPos = Origin;

			Position += Velocity * Time.Delta;
			Velocity += new Vector2( 0, 500 ) * Time.Delta;

			var screenPosVec2 = new Vector2( screenPos.x, screenPos.y );
			var screenPosPixels = screenPosVec2 * Screen.Size * ScaleFromScreen;

			Style.Left = Length.Pixels( screenPosPixels.x + Position.x );
			Style.Top = Length.Pixels( screenPosPixels.y + Position.y );
			Style.Dirty();
		}

		async Task TransitionOut()
		{
			await Task.Delay( 10000 );
			Delete();
		}
	}
}
