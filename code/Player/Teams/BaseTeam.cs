using Sandbox;
using System;
using System.Collections.Generic;

namespace Instagib.Teams
{
	public partial class BaseTeam : BaseNetworkable
	{
		[Net] public string TeamColor { get; set; }
		[Net] public int TeamId { get; set; }
		[Net] public string TeamName { get; set; }

		public bool AreFriendly( Player a, Player b )
		{
			return a.Team.Equals( b.Team );
		}

		public bool AreEnemies( Player a, Player b )
		{
			return !AreFriendly( a, b );
		}

		public BaseTeam Clone()
		{
			return new BaseTeam()
			{
				TeamName = TeamName,
				TeamId = TeamId,
				TeamColor = TeamColor,
			};
		}

		public override bool Equals( object obj )
		{
			if ( obj is BaseTeam team )
			{
				return team.TeamId == TeamId;
			}

			return false;
		}

		public static bool operator ==( BaseTeam a, BaseTeam b ) => a.Equals( b );
		public static bool operator !=( BaseTeam a, BaseTeam b ) => !a.Equals( b );
	}
}
