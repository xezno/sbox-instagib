using System.Collections.Generic;
using Instagib.GameStates;
using Sandbox;

namespace Instagib
{
	public partial class Game
	{
		/// <summary>
		/// Don't use this on the client cos it's server-only
		/// </summary>
		public BaseGameState CurrentState { get; set; } = new WaitingForPlayersState();

		//
		// Networked state variables
		//
		[Net] public string CurrentStateName { get; set; }
		[Net] public string CurrentStateTime { get; set; }
		[Net, Change( "(a, b) => Instagib.UI.InstagibHud.ToggleEndGameScreen( a, b )" )] public bool ShowEndGameScreen { get; set; }
		[Net] public bool InPlay { get; set; }

		public struct MapVote
		{
			public int MapIndex { get; set; }
			public long PlayerId { get; set; }
			public MapVote( int mapIndex, long playerId )
			{
				MapIndex = mapIndex;
				PlayerId = playerId;
			}
		}

		[Net] public IList<MapVote> MapVotes { get; set; }

		[ServerCmd( "vote_reset" )]
		public static void VoteReset()
		{
			Game.Instance.MapVotes.Clear();
		}

		[ServerCmd( "vote_map" )]
		public static void VoteMap( int index )
		{
			var steamId = ConsoleSystem.Caller.PlayerId;

			var mapVotes = Game.Instance?.MapVotes;

			foreach ( var mapVote in mapVotes )
				if ( mapVote.PlayerId == steamId )
					return;

			Game.Instance?.MapVotes.Add( new MapVote( index, steamId ) );

			Log.Trace( $"Voted for {index}" );
		}

		[ServerCmd( "vote_fake" )]
		public static void VoteFake( int index )
		{
			if ( !InstagibGlobal.DebugMode )
				return;

			Game.Instance?.MapVotes.Add( new MapVote( index, Rand.Int( 0, 1000000000 ) ) );
			Log.Trace( $"Voted for {index}" );
		}


		[Sandbox.Event.Tick]
		public void OnTick()
		{
			if ( Host.IsClient ) return;

			CurrentStateName = CurrentState.StateName();
			CurrentStateTime = CurrentState.StateTime();

			InPlay = CurrentState is MainGameState;
			ShowEndGameScreen = CurrentState is GameFinishedState;

			CurrentState.Tick();
		}
	}
}
