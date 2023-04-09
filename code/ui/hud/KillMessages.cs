namespace Instagib;

public class KillMessages : Panel
{
	class Message : Panel
	{
		public Message( string victimName )
		{
			Add.Panel( "background" );

			string text = $"You fragged {victimName}";

			if ( Game.Random.Next( 0, 100 ) == 69 )
				text = $"You shagged {victimName}";

			Add.Label( text, "small" );
			_ = LiveFor( 3.0f );
		}

		private async Task LiveFor( float seconds )
		{
			await Task.DelaySeconds( seconds );
			this.Delete();
		}
	}

	public KillMessages()
	{
		StyleSheet.Load( "/ui/hud/KillMessages.scss" );
	}

	[InstagibEvent.Player.Kill]
	public void OnKill( Player victim, DamageInfo damageInfo )
	{
		var message = new Message( victim.Client.Name );
		message.Parent = this;
	}
}
