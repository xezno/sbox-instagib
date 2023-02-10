namespace Instagib;

[UseTemplate]
internal class Speed : Panel
{
	public Label PlayerSpeedLabel { get; set; }

	public override void Tick()
	{
		base.Tick();

		//if ( Game.LocalPawn == null )
		//	return;

		//float vel = Game.LocalPawn.Velocity.WithZ( 0 ).Length;
		//float absSpeed = MathF.Abs( vel );
		//float roundedSpeed = absSpeed.FloorToInt();

		//if ( PlayerSpeedLabel.Text == null )
		//	return;

		//PlayerSpeedLabel.Text = $"{roundedSpeed}";
	}
}
