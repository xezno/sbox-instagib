using Instagib.Teams;
using Sandbox;

namespace Instagib.Entities
{
	[Library( "ent_flag" )]
	public class FlagEntity : ModelEntity
	{
		public PickupTrigger PickupTrigger { get; set; }

		public BaseTeam Team { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/flag/flag.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			SetInteractsAs( CollisionLayer.Debris );
			SetInteractsExclude( CollisionLayer.Player );

			EnableTouch = true;

			PickupTrigger = new();
			PickupTrigger.Parent = this;
			PickupTrigger.Position = Position;
			PickupTrigger.EnableTouch = true;

			var player = Client.All[0];
			Team = player.GetTeam();
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player player )
			{
				if ( player.Team != Team )
				{
					// Log.Trace( $"{player} should pick this flag up!! TODO " );
					SetParent( player, null, new Transform( Vector3.Backward * 32f ) );
				}
			}
		}

		[Event.Tick.Server]
		public void OnTick()
		{
			Rotation = Rotation.Identity;

			if ( Parent == null )
				return;

			if ( Parent.LifeState == LifeState.Dead )
				SetParent( null );
		}
	}
}
