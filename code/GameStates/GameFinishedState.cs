using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Instagib.GameStates
{
	public class GameFinishedState : BaseGameState
	{
		public override string StateName() => "Post-game";

		private RealTimeUntil stateEnds;

		public GameFinishedState()
		{
			stateEnds = 15;

			if ( InstagibGlobal.DebugMode )
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
}
