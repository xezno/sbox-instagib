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
		private TimeSince timeSinceCreated;
		
		public FragMessage( string target, string[] medals )
		{
			StyleSheet.Load( "/Code/UI/InstagibHud.scss" );
			
			SetClass( "frag", true );
			var fragMessage = AddChild<Panel>();
			
			fragMessage.SetClass( "frag-message", true );
			fragMessage.AddChild<Label>().SetText( Rand.Int(0, 10) == 1 ? "YOU SHAGGED " : "YOU FRAGGED " );
			fragMessage.AddChild<Label>().SetText( target );

			var medalPanel = AddChild<Panel>();
			medalPanel.SetClass( "medals", true );
			foreach ( string medal in medals )
				medalPanel.AddChild<Label>().SetText( medal );

			timeSinceCreated = 0;
		}

		public override void Tick()
		{
			base.Tick();
			if ( timeSinceCreated > 1.0f )
			{
				SetClass( "inactive", true );
			}
		}
	}
}
