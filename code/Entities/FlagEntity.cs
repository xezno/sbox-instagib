using Instagib.Teams;
using Sandbox;
using System.Linq;

namespace Instagib.Entities
{
	[Library( "ent_flag" )]
	public class FlagEntity : ModelEntity
	{
		public PickupTrigger PickupTrigger { get; set; }

		private BaseTeam team;
		public BaseTeam Team
		{
			get => team; 
			set
			{
				team = value;
				SetMaterialGroup( Team.TeamName + "Flag" );
			}
		}

		public bool HasBeenMoved { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/flag/flag.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			SetInteractsAs( CollisionLayer.Empty );
			SetInteractsExclude( CollisionLayer.Player );

			EnableTouch = true;

			PickupTrigger = new();
			PickupTrigger.Parent = this;
			PickupTrigger.Position = Position;
			PickupTrigger.EnableTouch = true;

			HasBeenMoved = false;

			Health = 1;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player player )
			{
				if ( player.Team == Team )
				{
					if ( HasBeenMoved )
					{
						// Return this flag
						TakeDamage( DamageInfo.Generic( 10000 ) );
					}
					else
					{
						// Check if we have a flag to capture
						if ( player.Children.Any( e => e is FlagEntity ) )
						{
							var playerCarryingEntity = player.Children.First( e => e is FlagEntity );
							if ( playerCarryingEntity.IsValid() && playerCarryingEntity is FlagEntity playerCarryingFlag )
							{
								// Capture flag
								playerCarryingEntity.TakeDamage( DamageInfo.Generic( 10000 ) );
								ClassicChatBox.AddInformation( To.Everyone, $"The {playerCarryingFlag.Team.TeamName} flag has been captured!" );
								// Tell the game to add score
								// Game.Instance.GameType.AddScore( Team, 1 );
							}
						}
					}
				}
				else if ( player.Team != Team ) // Other player grabs flag
				{
					SetParent( player, null, new Transform( Vector3.Backward * 32f ) );
					EnableAllCollisions = false;
					HasBeenMoved = true;
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

			if ( Position.z < -2500 )
			{
				TakeDamage( DamageInfo.Generic( 10000 ) );
			}
		}
	}
}
