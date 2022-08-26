namespace Instagib;

[UseTemplate]
internal class Speed : Panel
{
	public Label PlayerSpeedLabel { get; set; }
	
	public override void Tick()
	{
		base.Tick();

		float vel = Local.Pawn.Velocity.WithZ( 0 ).Length;
		float absSpeed = MathF.Abs( vel );
		float roundedSpeed = absSpeed.FloorToInt();
		
		PlayerSpeedLabel.Text = $"{roundedSpeed}";
	}
}
