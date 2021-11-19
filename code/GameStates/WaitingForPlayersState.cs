namespace Instagib.GameStates
{
	public class WaitingForPlayersState : BaseGameState
	{
		public override string StateName() => "Waiting for players";
		public override string StateTime() => $"{GetPlayerCount()} / 2";

		public override void Tick()
		{
			base.Tick();

			if ( GetPlayerCount() > 1 )
			{
				// Countdown then start game
				SetState( new WarmupState() );
			}
		}
	}
}
