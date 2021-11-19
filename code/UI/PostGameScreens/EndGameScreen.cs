using Instagib.UI.PostGameScreens.Elements;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.PostGameScreens
{
	public class EndGameScreen : BasePostGameScreen
	{
		private Label TimeLeftLabel { get; set; }

		public EndGameScreen() : base()
		{
			StyleSheet.Load( "/Code/UI/ParallaxHud.scss" );
			StyleSheet.Load( "/Code/UI/PostGameScreens/EndGameScreen.scss" );

			var winnerPanel = Add.Panel( "winner-section" );
			Game.Instance.GameType.CreateWinnerElements( this, winnerPanel );

			var votePanel = Add.Panel( "vote-section" );
			votePanel.Add.Label( $"VOTE NEXT MAP", "title" );

			TimeLeftLabel = votePanel.Add.Label( "", "time-left" );

			var mapList = votePanel.Add.Panel( "map-list" );

			for ( int i = 0; i < InstagibGlobal.GetMaps().Length; i++ )
			{
				string mapName = InstagibGlobal.GetMaps()[i];
				var mapPanel = MapVotePanel.FromPackage( mapName, i );
				mapPanel.Parent = mapList;
			}
		}

		public override void Tick()
		{
			base.Tick();
			TimeLeftLabel.Text = "Time left: " + Game.Instance?.CurrentStateTime;
		}

		public void CreateWinnerParticles( int particleCount )
		{
			particleCount = particleCount.Clamp( 0, 128 );

			for ( int i = 0; i < particleCount; ++i )
			{
				var particle = new WinnerParticle();
				var rand = (Vector2.Random + Vector2.Random + Vector2.Random + Vector2.Random) * new Vector2( 0.5f, 1.0f );
				particle.Origin = new Vector2( 0.5f, -2.0f ) + rand;
				particle.Parent = this;
			}
		}
	}
}
