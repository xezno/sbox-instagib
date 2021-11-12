using Instagib.Entities;
using Instagib.GameTypes;
using Instagib.Teams;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Instagib.UI.Elements
{
	[UseTemplate]
	public class FlagInfo : Panel
	{
		private Label RedCaptureLabel { get; set; }
		private Label BlueCaptureLabel { get; set; }
		private Label PlayingAsLabel { get; set; }

		private Label CurrentlyCarryingLabel { get; set; }

		public FlagInfo()
		{
			var localTeam = Local.Client.GetTeam();

			PlayingAsLabel.Text = $"You are playing as {localTeam.TeamName}";
			PlayingAsLabel.AddClass( localTeam.TeamName );

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

			var flagEntity = Local.Pawn.Children.FirstOrDefault( e => e is FlagEntity );
			if ( flagEntity.IsValid() && flagEntity is FlagEntity flag && CurrentlyCarryingLabel == null )
			{
				CurrentlyCarryingLabel = Add.Label( $"You have the {flag.Team.TeamName} flag", $"has-flag {flag.Team.TeamName}" );
			}
			else if ( !flagEntity.IsValid() && CurrentlyCarryingLabel != null )
			{
				CurrentlyCarryingLabel.Delete();
				CurrentlyCarryingLabel = null;
			}
		}
	}
}
