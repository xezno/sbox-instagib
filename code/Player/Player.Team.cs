using Sandbox;
using Instagib.Teams;

namespace Instagib
{
	public partial class Player
	{
		[Net] public BaseTeam Team { get; set; }

		public bool IsFriendly( Client otherClient )
		{
			if ( otherClient.Pawn is Player otherPlayer )
			{
				return Team.AreFriendly( this, otherPlayer );
			}
			return false;
		}

		public bool IsFriendly( Entity otherEntity )
		{
			if ( otherEntity is Player otherPlayer )
			{
				return Team.AreFriendly( this, otherPlayer );
			}
			return false;
		}

		public bool IsFriendly( Player otherPlayer )
		{
			return Team.AreFriendly( this, otherPlayer );
		}

		public bool IsEnemy( Player other )
		{
			return !IsFriendly( other );
		}

		public bool IsEnemy( Client other )
		{
			return !IsFriendly( other );
		}

		public bool IsEnemy( Entity other )
		{
			return !IsFriendly( other );
		}
	}
}
