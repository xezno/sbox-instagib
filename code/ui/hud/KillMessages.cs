namespace Instagib;

[UseTemplate]
public class KillMessages : Panel
{
	class Message : Panel
	{
		public Message( string victimName )
		{
			Add.Label( $"You fragged {victimName}", "small" );
			_ = LiveFor( 3.0f );
		}

		private async Task LiveFor( float seconds )
		{
			await Task.DelaySeconds( seconds );
			this.Delete();
		}
	}

	[InstagibEvent.Player.Kill]
	public void OnKill( Player victim, DamageInfo damageInfo )
	{
		var message = new Message( victim.Client.Name );
		message.Parent = this;
	}
}
