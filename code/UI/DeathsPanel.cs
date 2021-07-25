using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI
{

	/*
	<div class="frag">
		<text>YOU FRAGGED </text>
		<text class="highlight">Austin</text>
	</div>
	 */

	public class DeathsPanel : Panel
	{
		public static DeathsPanel Instance { get; private set; }
		public DeathsPanel()
		{
			Instance = this;
			
			SetClass( "frag", true );
		}

		public void AddDeathMessage( string weapon, string target )
		{
			foreach ( var child in Children?.Where( c => c is DeathMessage ) )
			{
				child?.Delete();
			}
			
			var deathMessage = new DeathMessage( weapon, target );
			deathMessage.Parent = this;
		}
	}
	
	public class DeathMessage : Panel
	{
		private Label skullLabel;

		public DeathMessage( string weapon, string target )
		{
			SetClass( "frag-message", true );
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			
			skullLabel = Add.Label( "💀", "frag-skull" );
			var fragDetails = Add.Panel( "frag-details" );
			
			fragDetails.Add.Label( $"FRAGGED BY" );
			fragDetails.Add.Label( $"{target}", "player" );
			fragDetails.Add.Label( $"{weapon}", "weapon" );
			
			//
			// Timeout
			//
			KillAfterTime();
		}

		async Task KillAfterTime()
		{
			await Task.Delay( 2500 );
			Delete();
		}
	}
}
