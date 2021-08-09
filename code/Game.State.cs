﻿using System;
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

			protected int GetPlayerCount() => Client.All.Count();
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
				foreach ( var client in Client.All )
				{
					var player = client.Pawn as Player;
					player?.Respawn();

					// Reset scores
					client.SetScore( "kills", 0 );
					client.SetScore( "deaths", 0 );
					client.SetScore( "totalShots", 0 );
					client.SetScore( "totalHits", 0 );
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

		private class MainGameState : BaseGameState
		{
			public override string StateName() => "Deathmatch";

			private RealTimeUntil stateEnds;

			public MainGameState() : base()
			{
				// Respawn players
				foreach ( var entity in Client.All )
				{
					var player = entity.Pawn as Player;
					player?.Respawn();

					// Reset scores
					entity.SetScore( "kills", 0 );
					entity.SetScore( "deaths", 0 );
					entity.SetScore( "totalShots", 0 );
					entity.SetScore( "totalHits", 0 );
				}

				stateEnds = 5 * 60;
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
				var orderedEntities = new List<Client>( Client.All );
				var orderedEnumerable =
					orderedEntities.OrderByDescending( p => p.GetScore( "kills", 0 ) );

				ClassicChatBox.AddInformation( To.Everyone,
					$"{orderedEnumerable.First().Name} wins!" );
				
				// Pause player movement
				foreach ( var client in Client.All )
				{
					((client.Pawn as Player).Controller as PlayerController).CanMove = false;
					(client.Pawn as Player).Inventory.DeleteContents();
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