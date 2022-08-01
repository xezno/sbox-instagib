namespace Instagib;

public class InstagibEvent
{
	public class Player
	{
		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class Kill : EventAttribute
		{
			public const string Name = "instagibevent.player.kill";

			public Kill() : base( Name ) { }
		}

		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class Death : EventAttribute
		{
			public const string Name = "instagibevent.player.death";

			public Death() : base( Name ) { }
		}

		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class DidDamage : EventAttribute
		{
			public const string Name = "instagibevent.player.diddamage";

			public DidDamage() : base( Name ) { }
		}
	}

	public class Game
	{
		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class Kill : EventAttribute
		{
			public const string Name = "instagibevent.game.kill";

			public Kill() : base( Name ) { }
		}
	}
}
