using Instagib.GameTypes;
using Sandbox;

namespace Instagib.Entities
{
	[Library( "ent_flag_podium" )]
	public class FlagPodiumEntity : ModelEntity
	{
		public enum Team
		{
			Blue,
			Red
		}

		[Property]
		public Team FlagTeam { get; set; }

		private FlagEntity flag;

		public override void Spawn()
		{
			base.Spawn();
			//SetModel( "models/flag/flag_podium.vmdl" );
			//SetupPhysicsFromModel( PhysicsMotionType.Static );

			SpawnFlag();
		}

		private void SpawnFlag()
		{
			flag?.Delete();

			flag = new FlagEntity();
			flag.Position = Position;

			// HACK? Needs revisiting if we ever need the flag for more than ctf
			if ( Game.Instance.GameType is CtfGameType ctfGame )
			{
				if ( FlagTeam == Team.Blue )
					flag.Team = ctfGame.BlueTeam;
				else if ( FlagTeam == Team.Red )
					flag.Team = ctfGame.RedTeam;
			}
		}

		[Event.Tick.Server]
		public void OnTick()
		{
			if ( !flag.IsValid() || flag.LifeState == LifeState.Dead )
			{
				SpawnFlag();
			}
		}
	}
}
