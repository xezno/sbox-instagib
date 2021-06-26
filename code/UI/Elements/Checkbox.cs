using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Elements
{
	public class Checkbox : Panel
	{
		private Panel checkMark;

		private bool value;

		public new bool Value
		{
			get => value;
			set
			{
				this.value = value;
				
				if ( value )
				{
					checkMark.AddClass( "visible" );
				}
				else
				{
					checkMark.RemoveClass( "visible" );
				}
			}
		}

		public delegate void CheckboxChangeEvent( bool value );

		public CheckboxChangeEvent OnValueChange;
		
		public Checkbox()
		{
			StyleSheet.Load( "/Code/UI/Elements/Checkbox.scss" );

			checkMark = Add.Icon( "check" );
			checkMark.AddClass( "checkmark" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			Value = !Value;
			OnValueChange?.Invoke( Value );
		}
	}
}
