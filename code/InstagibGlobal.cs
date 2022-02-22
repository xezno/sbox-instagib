using Sandbox;

namespace Instagib
{
	internal class InstagibGlobal
	{
		public const long AlexSteamId = 76561198128972602;

		[ServerVar]
		public static bool DebugMode { get; set; } = false;


		// TODO: Move to game type
		public static int SortClients( Client a, Client b )
		{
			var aKills = a.GetInt( "kills", 0 );
			var bKills = b.GetInt( "kills", 0 );

			if ( bKills > aKills )
				return 1;
			if ( aKills > bKills )
				return -1;

			return 0;
		}

		public static string[] GetMaps()
		{
			var packageTask = Package.Fetch( "alex.instagib", true ).ContinueWith( t =>
			{
				var package = t.Result;
				return package.GameConfiguration.MapList.ToArray();
			} );

			return packageTask.Result;
		}
	}
}
