using Sandbox;

namespace Instagib
{
	partial class Railgun
	{
		public TimeSince TimeSinceDischarge { get; set; }

		[Net, Predicted] public bool isGrappling { get; set; }
		[Net] public GrappleHookEntity grappleHookEntity { get; set; }
		[Net, Predicted] private TimeSince TimeSinceLastGrapple { get; set; }

		private Particles grappleParticles;

		private float GrappleCooldown => 3f;
		private float HookSpeed => 5f;
		private float PullStrength => 20f;
		private float BoostStrength => 4f;
		private float AntiVelocityScale => 1f;
		private float MaxDistance => 2500f;


		public void GrappleSimulate( Client owner )
		{
			if ( Input.Pressed( InputButton.Use ) && !isGrappling && TimeSinceLastGrapple > GrappleCooldown )
				DeployGrapple();
			else if ( !Input.Down( InputButton.Use ) && isGrappling )
				RemoveGrapple();

			if ( isGrappling )
			{
				if ( Owner is Player { Controller: PlayerController controller } player )
				{
					player.GroundEntity = null;
					controller.ClearGroundEntity();
				}

				Vector3 playerVel = Owner.Velocity;
				Vector3 playerLookDir = Owner.EyeRot.Forward;
				Owner.Velocity += playerLookDir * BoostStrength;

				Vector3 playerLatchDir = (Owner.Position - grappleHookEntity.Position).Normal;
				Owner.Velocity -= playerLatchDir * PullStrength;

				float projLength = playerVel.Dot( playerLatchDir );
				if ( projLength > 0 )
				{
					Vector3 projVector = projLength * playerLatchDir;
					Owner.Velocity -= AntiVelocityScale * projVector;
				}
			}
		}

		protected virtual void DeployGrapple()
		{
			if ( Host.IsServer )
			{
				var tr = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward * MaxDistance )
					.Ignore( this )
					.WorldOnly()
					.Run();
				if ( tr.Hit && tr.Entity != null )
				{
					isGrappling = true;

					using ( Prediction.Off() )
					{
						// PlaySound( "grapple" );

						if ( Owner is Player { Controller: PlayerController controller } player )
						{
							if ( controller.GroundEntity != null )
							{
								player.Velocity += Vector3.Up * 128;
							}

							player.GroundEntity = null;
							controller.ClearGroundEntity();
						}

						grappleHookEntity = new()
						{
							Position = tr.StartPos,
							Target = tr.EndPos,
							HookSpeed = HookSpeed,
							WorldAng = Owner.EyeRot.Angles(),
							Parent = tr.Entity,
							Owner = Owner
						};
						grappleHookEntity.Spawn();

						var rope = Particles.Create( "particles/grapple_beam.vpcf" );
						rope.SetEntity( 0, grappleHookEntity, Vector3.Backward * 32 );
						rope.SetEntity( 1, Owner, Vector3.Up * 32 );

						grappleParticles = rope;
					}
				}
				else
				{
					using ( Prediction.Off() )
						PlaySound( "player_use_fail" );
				}
			}
		}

		protected virtual void RemoveGrapple()
		{
			isGrappling = false;
			TimeSinceLastGrapple = 0;

			if ( IsServer )
			{
				DeleteHook();
			}
		}

		protected virtual void DeleteHook()
		{
			if ( !IsServer )
				return;

			if ( grappleHookEntity != null && grappleHookEntity.IsValid() )
			{
				grappleHookEntity.Delete();
			}

			grappleHookEntity = null;

			grappleParticles?.Destroy( true );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DeleteHook();
		}
	}
}
