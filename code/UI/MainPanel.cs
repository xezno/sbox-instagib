using Instagib.UI.Elements;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		public Label MenuPrompt { get; set; }
		public string MenuPromptText => $"Press {Input.GetKeyWithBinding( "iv_menu" ).ToUpper()} to open the menu";
		private float PlayerSpeed { get; set; }
		
		public string PlayerHealthText => $"{Local.Client.Pawn.Health.CeilToInt()}";
		public string PlayerSpeedText => $"{PlayerSpeed:N0}u/s";

		public string GameStateText => Game.Instance?.CurrentStateName;
		public string GameStateTime => Game.Instance?.CurrentStateTime;

		public MainPanel()
		{
			SetTemplate( "/Code/UI/MainPanel.html" );
			StyleSheet.Load( "/Code/UI/MainPanel.scss" ); // Loading in HTML doesn't work for whatever reason
			SetClass( "mainpanel", true );
		}

		public override void Tick()
		{
			base.Tick();

			PlayerSpeed = Local.Pawn.Velocity.Cross( Vector3.Up ).Length;

			MenuPrompt?.SetClass( "visible", PlayerSpeed < 30f );
		}
	}
}
