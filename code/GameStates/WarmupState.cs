using System;
using Sandbox;

namespace Instagib.GameStates
{
	public class WarmupState : BaseGameState
	{
		public override string StateName() => "Warmup";

		private RealTimeUntil stateEnds;

		public WarmupState() : base()
		{
			// Respawn players
			foreach ( var client in Client.All )
			{
				var player = client.Pawn as Player;
				player?.Respawn();

				// Reset scores
				client.SetInt( "kills", 0 );
				client.SetInt( "deaths", 0 );
				client.SetInt( "totalShots", 0 );
				client.SetInt( "totalHits", 0 );
			}

			stateEnds = 10;

			if ( InstagibGlobal.DebugMode )
				stateEnds = 1;
		}

		public override string StateTime()
		{
			var timeEndSpan = TimeSpan.FromSeconds( stateEnds );
			var minutes = timeEndSpan.Minutes;
			var seconds = timeEndSpan.Seconds;
			return $"{minutes:D2}:{seconds:D2}";
		}

		private bool playedCountdown = false;

		public override void Tick()
		{
			base.Tick();

			if ( GetPlayerCount() <= 1 )
			{
				SetState( new WaitingForPlayersState() );
			}

			if ( stateEnds <= 4 && !playedCountdown )
			{
				playedCountdown = true;
				Sound.FromScreen( "countdown" );
			}

			if ( stateEnds < 0 )
			{
				SetState( new MainGameState() );
			}
		}
	}
}
