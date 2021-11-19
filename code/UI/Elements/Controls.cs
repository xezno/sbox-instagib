using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class Controls : Panel
	{
		public Controls()
		{
			StyleSheet.Load( "/Code/UI/Elements/Controls.scss" );
			Add.Label( "Controls:", "title" );
			AddControl( "Grapple", "iv_duck" );
			AddControl( "Zoom", "iv_sprint" );
			AddControl( "Shoot", "iv_attack");
			AddControl( "Rocket jump", "iv_attack2" );
		}

		private void AddControl( string name, string binding )
		{
			var panel = Add.Panel( "control" );
			panel.Add.Label( name );
			panel.Add.Label( Input.GetKeyWithBinding( binding ) );
		}
	}
}
