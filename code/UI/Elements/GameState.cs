using Sandbox.UI;

namespace Instagib.UI.Elements
{
	[UseTemplate]
	public class GameState : Panel
	{
		public string GameStateText => Game.Instance?.CurrentStateName;
		public string GameStateTime => Game.Instance?.CurrentStateTime;

		public GameState()
		{

		}
	}
}
