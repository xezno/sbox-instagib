using Sandbox;

namespace Instagib.Teams
{
	public partial class TeamComponent : EntityComponent
	{
		[Net] public BaseTeam Team { get; set; }

		public override bool CanAddToEntity( Entity entity )
		{
			if ( entity.Components.TryGet<TeamComponent>( out _ ) )
				return false;

			return base.CanAddToEntity( entity );
		}
	}
}
