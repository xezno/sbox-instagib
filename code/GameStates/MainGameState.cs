using System;
using Sandbox;

namespace Instagib.GameStates
{
	public class MainGameState : BaseGameState
	{
		public override string StateName() => Game.Instance.GameType.GameTypeName;

		private RealTimeUntil stateEnds;

		public MainGameState() : base()
		{
			// Respawn players
			foreach ( var entity in Client.All )
			{
				var player = entity.Pawn as Player;
				player?.Respawn();

				// Reset scores
				entity.SetInt( "kills", 0 );
				entity.SetInt( "deaths", 0 );
				entity.SetInt( "totalShots", 0 );
				entity.SetInt( "totalHits", 0 );
			}

			stateEnds = 5 * 60;

			if ( InstagibGlobal.DebugMode )
				stateEnds = 5 * 60 * 1000;
			else
				GameServices.StartGame();
		}

		public override void OnKill( Client attackerClient, Client victimClient )
		{
			base.OnKill( attackerClient, victimClient );

			if ( !InstagibGlobal.DebugMode )
				GameServices.RecordEvent( attackerClient, "killed", victim: victimClient );
		}

		public override void OnDeath( Client cl )
		{
			base.OnDeath( cl );

			if ( !InstagibGlobal.DebugMode )
				GameServices.RecordEvent( cl, "died" );
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

			if ( Game.Instance.GameType.GameShouldEnd() || GetPlayerCount() <= 1 ||
				stateEnds < 0 )
			{
				SetState( new GameFinishedState() );

				if ( !InstagibGlobal.DebugMode )
					GameServices.EndGame();

				return;
			}
		}
	}
}
