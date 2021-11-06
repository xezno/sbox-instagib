using Sandbox.UI;

namespace Instagib.UI.PostGameScreens
{
	public class BasePostGameScreen : Panel
	{
		public BasePostGameScreen()
		{
			AddClass( "post-game-screen" );
			StyleSheet.Load( "/Code/UI/PostGameScreens/BasePostGameScreen.scss" );
		}
	}
}
