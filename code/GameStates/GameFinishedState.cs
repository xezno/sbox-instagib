using System;
using Sandbox;

namespace Instagib.GameStates
{
	public class GameFinishedState : BaseGameState
	{
		public override string StateName() => "Post-game";

		private RealTimeUntil stateEnds;

		public GameFinishedState()
		{
			// Pause player movement
			foreach ( var client in Client.All )
			{
				((client.Pawn as Player).Controller as PlayerController).CanMove = false;
				(client.Pawn as Player).Inventory.DeleteContents();
			}

			stateEnds = 10;
		}

		public override string StateTime()
		{
			var timeEndSpan = TimeSpan.FromSeconds( stateEnds );
			var minutes = timeEndSpan.Minutes;
			var seconds = timeEndSpan.Seconds;
			return $"{minutes:D2}:{seconds:D2}";
		}

		public override void Tick()
		{
			base.Tick();

			if ( stateEnds < 0 )
			{
				SetState( new MapVoteState() );
			}
		}
	}
}
