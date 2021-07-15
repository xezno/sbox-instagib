using Sandbox;

namespace Instagib.Utils
{
	public static class Event
	{
		public const string PlayerJoined = "playerJoined";
		public const string PlayerLeft = "playerLeft";
		public const string PlayerKilled = "playerKilled";
		public const string PlayerRespawn = "playerRespawn";
		public const string PlayerChat = "playerChat";
		public const string PlayerDeath = "playerDeath";

		public class PlayerJoinedAttribute : EventAttribute
		{
			public PlayerJoinedAttribute() : base( PlayerJoined ) { }
		}

		public class PlayerLeftAttribute : EventAttribute
		{
			public PlayerLeftAttribute() : base( PlayerLeft ) { }
		}

		public class PlayerKilledAttribute : EventAttribute
		{
			public PlayerKilledAttribute() : base( PlayerKilled ) { }
		}

		public class PlayerRespawnAttribute : EventAttribute
		{
			public PlayerRespawnAttribute() : base( PlayerRespawn ) { }
		}

		public class PlayerChatAttribute : EventAttribute
		{
			public PlayerChatAttribute() : base( PlayerChat ) { }
		}

		public class PlayerDeathAttribute : EventAttribute
		{
			public PlayerDeathAttribute() : base( PlayerDeath ) { }
		}
		
	}
}
