using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	/*
	<div class="frag">
		<text>YOU FRAGGED </text>
		<text class="highlight">Austin</text>
	</div>
	 */
	
	public class FragMessage : Panel
	{
		public FragMessage( string target, string[] medals )
		{
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
			
			SetClass( "frag", true );
			var fragMessage = AddChild<Panel>();
			
			fragMessage.SetClass( "frag-message", true );
			fragMessage.AddChild<Label>().SetText( Rand.Int(0, 10000) == 1 ? "YOU SHAGGED " : "YOU FRAGGED " ); // :)
			
			var playerText = fragMessage.AddChild<Label>();
			playerText.SetText( target );
			playerText.SetClass( "player", true );

			//
			// Medal display
			//
			var medalPanel = AddChild<Panel>();
			medalPanel.SetClass( "medals", true );
			
			foreach ( string medal in medals )
				medalPanel.AddChild<Label>().SetText( medal );
			
			KillAfterTime();
		}
		
		async Task KillAfterTime()
		{
			await Task.Delay( 1000 );
			Delete();
		}
	}
}
