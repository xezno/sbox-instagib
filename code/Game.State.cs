using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instagib.Utils;
using Sandbox;

namespace Instagib
{
	public partial class Game
	{
		public class BaseGameState
		{
			protected RealTimeSince stateStart;

			public BaseGameState()
			{
				stateStart = 0;
			}

			public virtual string StateName() => GetType().ToString();

			public virtual string StateTime()
			{
				var timeEndSpan = TimeSpan.FromSeconds( stateStart );
				var minutes = timeEndSpan.Minutes;
				var seconds = timeEndSpan.Seconds;
				return $"{minutes:D2}:{seconds:D2}";
			}

			public virtual void Tick() { }

			protected void SetState( BaseGameState newState )
			{
				(Current as Game).CurrentState = newState;
			}

			protected int GetPlayerCount() => All.Where( t => t is Player ).Count();
		}

		private class WaitingForPlayersState : BaseGameState
		{
			private bool isCountingDown;

			public override string StateName() => "Waiting for players";
			public override string StateTime() => $"{GetPlayerCount()} / 2";

			public override void Tick()
			{
				base.Tick();

				if ( isCountingDown ) return;

				// Check player count
				if ( GetPlayerCount() > 1 )
				{
					// Countdown then start game
					SetState( new WarmupState() );
				}
			}
		}

		private class WarmupState : BaseGameState
		{
			public override string StateName() => "Warmup";

			private RealTimeUntil stateEnds;

			public WarmupState() : base()
			{
				// Respawn players
				var entitiesCopy = Entity.All.ToArray();
				foreach ( var entity in entitiesCopy.Where( e => e is Player ) )
				{
					var player = entity as Player;
					player?.Respawn();

					// Reset scores
					entity.GetClientOwner().SetScore( "kills", 0 );
					entity.GetClientOwner().SetScore( "deaths", 0 );
					entity.GetClientOwner().SetScore( "totalShots", 0 );
					entity.GetClientOwner().SetScore( "totalHits", 0 );
				}

				stateEnds = 15;
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

				if ( GetPlayerCount() <= 1 )
				{
					SetState( new WaitingForPlayersState() );
				}

				if ( stateEnds < 0 )
				{
					SetState( new MainGameState() );
				}
			}
		}

		private class MainGameState : BaseGameState
		{
			public override string StateName() => "Deathmatch";

			private RealTimeUntil stateEnds;

			public MainGameState() : base()
			{
				// Respawn players
				var entitiesCopy = Entity.All.ToArray();
				foreach ( var entity in entitiesCopy.Where( e => e is Player ) )
				{
					var player = entity as Player;
					player?.Respawn();

					// Reset scores
					entity.GetClientOwner().SetScore( "kills", 0 );
					entity.GetClientOwner().SetScore( "deaths", 0 );
					entity.GetClientOwner().SetScore( "totalShots", 0 );
					entity.GetClientOwner().SetScore( "totalHits", 0 );
				}

				stateEnds = 2 * 60; // 2 minutes
				//stateEnds = 5;
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

				if ( GetPlayerCount() <= 1 )
				{
					SetState( new GameFinishedState() );
				}

				if ( stateEnds < 0 )
				{
					SetState( new GameFinishedState() );
				}
			}
		}

		private class GameFinishedState : BaseGameState
		{
			public override string StateName() => "Post-game";

			private RealTimeUntil stateEnds;

			public GameFinishedState()
			{
				// Determine winner
				var entitiesCopy = All.Where( t => t is Player ).ToArray();
				var orderedEntities = new List<Entity>( entitiesCopy );
				var orderedEnumerable =
					orderedEntities.OrderByDescending( p => p.GetClientOwner().GetScore( "kills", 0 ) );

				ClassicChatBox.AddInformation( To.Everyone,
					$"{orderedEnumerable.First().GetClientOwner().Name} wins!" );
				
				stateEnds = 15;
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
					SetState( new WaitingForPlayersState() );
				}
			}
		}

		/// <summary>
		/// Don't use this on the client cos it's server-only
		/// </summary>
		private BaseGameState CurrentState { get; set; } = new WaitingForPlayersState();

		//
		// Networked state variables
		//
		[Net] public string CurrentStateName { get; set; }
		[Net] public string CurrentStateTime { get; set; }

		[Sandbox.Event.Tick]
		public void OnTick()
		{
			if ( Host.IsClient ) return;

			CurrentStateName = CurrentState.StateName();
			CurrentStateTime = CurrentState.StateTime();
			
			CurrentState.Tick();
		}
	}
}
