using Instagib.GameTypes;
using Instagib.Teams;
using Sandbox;
using System.Linq;

namespace Instagib.Entities
{
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

		private void ReturnFlag()
		{
			ClassicChatBox.AddInformation( To.Everyone, $"The {Team.TeamName} flag has been returned!" );
			TakeDamage( DamageInfo.Generic( 10000 ) );
		}

		private void CaptureFlag( FlagEntity targetFlag )
		{
			targetFlag.TakeDamage( DamageInfo.Generic( 10000 ) );
			ClassicChatBox.AddInformation( To.Everyone, $"The {targetFlag.Team.TeamName} flag has been captured!" );

			// Team in this case is the team capturing the flag
			if ( Game.Instance.GameType is CtfGameType ctfGame )
			{
				ctfGame.CaptureFlag( Team );
			}
		}

		private void PickupFlag( Player player )
		{
			ClassicChatBox.AddInformation( To.Everyone, $"{player.Client.Name} picked up the {Team.TeamName} flag!" );
			SetParent( player, null, new Transform( Vector3.Backward * 32f ) );
			EnableAllCollisions = false;
			HasBeenMoved = true;
		}

		private void DropFlag()
		{
			ClassicChatBox.AddInformation( To.Everyone, $"The {Team.TeamName} flag has been dropped!" );
			SetParent( null );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( !Game.Instance.InPlay )
				return;

			if ( other is not Player player )
				return;

			if ( player.Team == Team )
			{
				if ( HasBeenMoved )
				{
					ReturnFlag();
				}
				else
				{
					// Check if we have a flag to capture
					if ( player.Children.Any( e => e is FlagEntity ) )
					{
						var playerCarryingEntity = player.Children.First( e => e is FlagEntity );
						if ( playerCarryingEntity.IsValid() && playerCarryingEntity is FlagEntity playerCarryingFlag )
						{
							CaptureFlag( playerCarryingFlag );
						}
					}
				}
			}
			else if ( player.Team != Team )
			{
				PickupFlag( player );
			}
		}

		[Event.Tick.Server]
		public void OnTick()
		{
			Rotation = Rotation.Identity;

			if ( Position.z < -2500 )
				TakeDamage( DamageInfo.Generic( 10000 ) );

			if ( Parent == null )
				return;

			if ( Parent.LifeState == LifeState.Dead )
				DropFlag();
		}
	}
}
