using Sandbox;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Instagib.Entities
{
	[Library( "info_ig_player_spawn" )]
	[SandboxEditor.Model( Model = "models/citizen/citizen.vmdl" )]
	[Display( Name = "Player Spawn" ), Category( "Instagib" ), Icon( "person" )]
	public partial class InstagibPlayerSpawn : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Never;
		}
	}
}
