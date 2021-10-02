using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI
{
	public class FragsPanel : Panel
	{
		public static FragsPanel Instance { get; private set; }
		public FragsPanel()
		{
			Instance = this;
			
			SetClass( "frag", true );
		}

		public void AddFragMessage( string weapon, string target, string[] medals )
		{
			foreach ( var child in Children?.Where( c => c is FragMessage ) )
			{
				child?.Delete();
			}
			
			var fragMessage = new FragMessage( weapon, target, medals );
			fragMessage.Parent = this;
		}
	}
	
	public class FragMessage : Panel
	{
		private Label skullLabel;

		public FragMessage( string weapon, string target, string[] medals )
		{
			// Log.Trace( "Frag message created"  );
			
			SetClass( "frag-message", true );
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );
			
			skullLabel = Add.Label( "💀", "frag-skull" );
			var fragDetails = Add.Panel( "frag-details" );
			
			fragDetails.Add.Label( $"{target}", "player" );
			fragDetails.Add.Label( $"{weapon}", "weapon" );

			//
			// Medal display
			//
			var medalPanel = fragDetails.Add.Panel( "medals" );
			
			foreach ( string medal in medals )
				medalPanel.AddChild<Label>().SetText( medal );
			
			//
			// Timeout
			//
			_ = KillAfterTime();
		}

		async Task KillAfterTime()
		{
			await Task.Delay( 2500 );
			Delete();
		}
	}
}
