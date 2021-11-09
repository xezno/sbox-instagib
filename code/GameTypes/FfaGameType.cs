using Sandbox;

namespace Instagib.GameTypes
{
	[Library( "gametype_ffa", Title = "Free for all", Description = "Basic deathmatch game type" )]
	public partial class FfaGameType : BaseGameType
	{
		public FfaGameType()
		{
			GameTypeName = "Free-for-all";
			GameTypeDescription = "";
			IsExperimental = false;
		}
	}
}
