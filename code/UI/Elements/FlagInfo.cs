using Instagib.GameTypes;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class FlagInfo : Panel
	{
		private Label RedCaptureLabel { get; set; }
		private Label BlueCaptureLabel { get; set; }

		public FlagInfo()
		{
			Add.Icon( "flag", "flag-icon red" );
			RedCaptureLabel = Add.Label( "0", "capture-count red" );

			Add.Panel( "separator" );

			Add.Icon( "flag", "flag-icon blue" );
			BlueCaptureLabel = Add.Label( "0", "capture-count blue" );

			StyleSheet.Load( "/Code/UI/Elements/FlagInfo.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Game.Instance.GameType is CtfGameType ctfGame )
			{
				RedCaptureLabel.Text = ctfGame.RedCaptures.ToString();
				BlueCaptureLabel.Text = ctfGame.BlueCaptures.ToString();
			}
		}
	}
}
