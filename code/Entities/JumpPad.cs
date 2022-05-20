using Sandbox;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Instagib.Entities
{
	[Library( "trigger_ig_jump_pad" )]
	[SandboxEditor.RenderFields]
	[SandboxEditor.Model( Model = "models/launch_pad/launch_pad.vmdl" )]
	[SandboxEditor.Line( "targetname", "targetentity" )]
	[Display( Name = "Jump Pad" ), Category( "Instagib" ), Icon( "arrow_upward" )]
	public partial class JumpPad : BaseTrigger
	{
		[Net, Property, FGDType( "target_destination" )] public string TargetEntity { get; set; } = "";
		[Net, Property] public float VerticalBoost { get; set; } = 200f;
		[Net, Property] public float Force { get; set; } = 1000f;

		public override void Spawn()
		{
			if ( Force == 0f )
			{
				Force = 1000f;
			}

			base.Spawn();
		}

		public override void Touch( Entity other )
		{
			if ( !other.IsServer ) return;
			if ( other is not Instagib.Player pl ) return;
			var target = FindByName( TargetEntity );

			if ( target.IsValid() )
			{
				var direction = (target.Position - other.Position).Normal;
				pl.ApplyForce( new Vector3( 0f, 0f, VerticalBoost ) );
				pl.ApplyForce( direction * Force );
			}

			base.Touch( other );
		}
	}
}
