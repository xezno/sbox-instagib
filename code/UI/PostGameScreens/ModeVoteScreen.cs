using Instagib.GameTypes;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;
using System.Threading.Tasks;

namespace Instagib.UI.PostGameScreens
{
	public class ModeVoteScreen : BasePostGameScreen
	{
		private Label TimeLeftLabel { get; set; }
		public ModeVoteScreen() : base()
		{
			StyleSheet.Load( "/Code/UI/PostGameScreens/ModeVoteScreen.scss" );

			Add.Label( $"VOTE NEXT MODE", "title" );

			TimeLeftLabel = Add.Label( "", "time-left" );

			var mapList = Add.Panel( "map-list" );

			var gameTypes = Library.GetAllAttributes<BaseGameType>().ToArray();
			for ( int i = 0; i < gameTypes.Length; i++ )
			{
				if ( gameTypes[i].Name == "BaseGameType" )
					continue;
				Log.Trace( gameTypes[i].Name );
				var mapPanel = new ModeVotePanel(
					gameTypes[i].Title,
					gameTypes[i].Description,
					true
				);
				mapPanel.Parent = mapList;
			}
		}

		public override void Tick()
		{
			base.Tick();
			TimeLeftLabel.Text = "Time left: " + Game.Instance?.CurrentStateTime;
		}
	}

	public class ModeVotePanel : Panel
	{
		private Label VoteCount { get; set; }

		public ModeVotePanel( string name, string description, bool isExperimental )
		{
			AddClass( "vote-panel" );
			VoteCount = Add.Label( "0", "vote-count" );

			Add.Label( "VOTES", "vote-subtext" );

			var bottom = Add.Panel( "mode-name-panel" );
			bottom.Add.Label( name, "mode-name" );

			if ( isExperimental )
				bottom.Add.Icon( "science", "experimental" );

			AddEventListener( "onclick", () =>
			{
				if ( HasClass( "disabled" ) )
					return;

				Sound.FromScreen( "vote_confirm" );
				_ = SetClickClass();
			} );
		}

		private async Task SetClickClass()
		{
			AddClass( "clicked" );
			await Task.Delay( 50 );
			RemoveClass( "clicked" );
		}
	}
}
