using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Instagib
{
	public partial class Game
	{
		[ServerVar]
		public static bool DebugMode { get; set; } = false;

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

			public virtual void OnKill( Client attackerClient, Client victimClient ) { }

			public virtual void OnDeath( Client cl ) { }

			public virtual void OnPlayerJoin( Client cl ) { }

			public virtual void OnPlayerLeave( Client cl ) { }
		}

		private class WaitingForPlayersState : BaseGameState
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
					client.SetInt( "kills", 0 );
					client.SetInt( "deaths", 0 );
					client.SetInt( "totalShots", 0 );
					client.SetInt( "totalHits", 0 );
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
					entity.SetInt( "kills", 0 );
					entity.SetInt( "deaths", 0 );
					entity.SetInt( "totalShots", 0 );
					entity.SetInt( "totalHits", 0 );
				}

				stateEnds = 5 * 60;

				if ( DebugMode )
					stateEnds = 5 * 60 * 1000;
				else
					GameServices.StartGame();
			}

			public override void OnKill( Client attackerClient, Client victimClient )
			{
				base.OnKill( attackerClient, victimClient );

				if ( !DebugMode )
					GameServices.RecordEvent( attackerClient, "killed", victim: victimClient );
			}

			public override void OnDeath( Client cl )
			{
				base.OnDeath( cl );

				if ( !DebugMode )
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

				if ( GetPlayerCount() <= 1 )
				{
					SetState( new GameFinishedState() );

					if ( !DebugMode )
						GameServices.EndGame();

					return;
				}

				if ( stateEnds < 0 )
				{
					SetState( new GameFinishedState() );

					if ( !DebugMode )
						GameServices.EndGame();

					return;
				}
			}
		}

		private class GameFinishedState : BaseGameState
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

		private class MapVoteState : BaseGameState
		{
			public override string StateName() => "Voting";

			private RealTimeUntil stateEnds;

			public MapVoteState()
			{
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
					Dictionary<int, int> mapVotePairs = new();
					foreach ( var mapVote in Game.Instance.MapVotes )
					{
						if ( !mapVotePairs.ContainsKey( mapVote.MapIndex ) )
							mapVotePairs.Add( mapVote.MapIndex, 0 );

						mapVotePairs[mapVote.MapIndex]++;
					}

					var sortedMapVotePairs = from entry in mapVotePairs orderby entry.Value descending select entry;
					if ( sortedMapVotePairs.Count() == 0 )
					{

						Global.ChangeLevel( InstagibGlobal.GetMaps()[0] );
					}

					var votedMap = sortedMapVotePairs.First();
					Global.ChangeLevel( InstagibGlobal.GetMaps()[votedMap.Key] );
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
		[Net, Change( "(a, b) => Instagib.UI.InstagibHud.ToggleWinnerScreen( a, b )" )] public bool ShowWinnerScreen { get; set; }
		[Net, Change( "(a, b) => Instagib.UI.InstagibHud.ToggleMapVoteScreen( a, b )" )] public bool ShowMapVoteScreen { get; set; }

		public struct MapVote
		{
			public int MapIndex { get; set; }
			public ulong PlayerId { get; set; }
			public MapVote( int mapIndex, ulong playerId )
			{
				MapIndex = mapIndex;
				PlayerId = playerId;
			}
		}

		[Net] public List<MapVote> MapVotes { get; set; }

		[ServerCmd( "vote_reset" )]
		public static void VoteReset()
		{
			Game.Instance.MapVotes.Clear();
		}

		[ServerCmd( "vote_map" )]
		public static void VoteMap( int index )
		{
			var steamId = ConsoleSystem.Caller.SteamId;

			var mapVotes = Game.Instance?.MapVotes;

			foreach ( var mapVote in mapVotes )
				if ( mapVote.PlayerId == steamId )
					return;

			Game.Instance?.MapVotes.Add( new MapVote( index, ConsoleSystem.Caller.SteamId ) );

			Log.Trace( $"Voted for {index}" );
		}

		[ServerCmd( "vote_fake" )]
		public static void VoteFake( int index )
		{
			if ( !DebugMode )
				return;

			Game.Instance?.MapVotes.Add( new MapVote( index, (ulong)Rand.Int( 0, 1000000000 ) ) );
			Log.Trace( $"Voted for {index}" );
		}


		[Sandbox.Event.Tick]
		public void OnTick()
		{
			if ( Host.IsClient ) return;

			CurrentStateName = CurrentState.StateName();
			CurrentStateTime = CurrentState.StateTime();

			ShowWinnerScreen = CurrentState is GameFinishedState;
			ShowMapVoteScreen = CurrentState is MapVoteState;

			CurrentState.Tick();
		}
	}
}
