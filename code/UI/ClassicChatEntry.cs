using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


public partial class ClassicChatEntry : Panel
{
	public Label NameLabel { get; internal set; }
	public Label Message { get; internal set; }
	public Image Avatar { get; internal set; }

	private RealTimeSince TimeSinceBorn = 0;

	public ClassicChatEntry()
	{
		Avatar = Add.Image();
		NameLabel = Add.Label( "Name", "name" );
		Message = Add.Label( "Message", "message" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( TimeSinceBorn > 3 )
		{
			Hide();
			//Delete();
		}

		//Log.Info( TimeSinceBorn.ToString() );
	}

	public void Hide()
	{
		AddClass( "hide" );
		//this.Style.BackgroundColor = new Vector4(0,0,0,0);
	}
}
