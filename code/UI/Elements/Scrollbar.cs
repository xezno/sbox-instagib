using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Elements
{
	public class Scrollbar : Panel
	{
		private class ScrollHandle : Panel
		{
			public ScrollHandle()
			{	
				StyleSheet.Load( "/Code/UI/Elements/Scrollbar.scss" );
			}
		}

		private ScrollHandle handle;
		
		public Panel Panel { get; set; }
		
		public Scrollbar()
		{
			StyleSheet.Load( "/Code/UI/Elements/Scrollbar.scss" );

			handle = AddChild<ScrollHandle>();
		}

		public override void Tick()
		{
			base.Tick();
			
			var scrollPos = Panel.ScrollOffset.y / Panel.ScrollSize.y;
			
			handle.Style.Top = (scrollPos * Box.Rect.height);
			handle.Style.Dirty();
		}	
	}
}
