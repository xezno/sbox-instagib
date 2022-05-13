using Instagib.GameTypes;
using Sandbox;

namespace Instagib.Entities
{
	/// <summary>
	/// This is where flags are held / spawned / etc.
	/// </summary>
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
			SpawnFlag();
		}

		private void SpawnFlag()
		{
			flag?.Delete();

			Log.Trace( "SpawnFlag" );

			flag = new FlagEntity();
			flag.Position = Position;

			// HACK? Needs revisiting if we ever need the flag for more than ctf
			if ( Game.Instance.GameType is CtfGameType ctfGame )
			{
				Log.Trace( $"CTFGame: {FlagTeam}" );
				if ( FlagTeam == Team.Blue )
				{
					flag.Team = ctfGame.BlueTeam;
				}
				else if ( FlagTeam == Team.Red )
				{
					flag.Team = ctfGame.RedTeam;
				}
				else
				{
					Log.Error( $"We don't have a team???" );
				}
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
