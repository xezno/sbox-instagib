using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class ClassicChatBox : Panel
{
	public static ClassicChatBox Current;

	public Panel Canvas { get; protected set; }
	public TextEntry Input { get; protected set; }

	public ClassicChatBox()
	{
		Current = this;

		StyleSheet.Load( "/Code/UI/ClassicChatBox.scss" );

		Canvas = Add.Panel( "classicchat_canvas" );
		Canvas.PreferScrollToBottom = true;

		Input = Add.TextEntry( "" );
		Input.AddEventListener( "onsubmit", () => Submit() );
		Input.AddEventListener( "onblur", () => Close() );
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;

		Chat.OnOpenChat += Open;
	}

	void Open()
	{
		AddClass( "open" );
		Input.Focus();
		
		foreach ( ClassicChatEntry message in Canvas.Children )
		{
			if ( message.HasClass( "hide" ) )
			{
				message.AddClass( "show" );
			}
		}

		Canvas.TryScrollToBottom();
	}

	void Close()
	{
		RemoveClass( "open" );
		Input.Blur();
		
		foreach ( ClassicChatEntry message in Canvas.Children )
		{
			if ( message.HasClass( "show" ) )
			{
				message.RemoveClass( "show" );
				message.AddClass( "expired" );
			}	
		}

		Canvas.TryScrollToBottom();
	}

	void Submit()
	{
		Close();

		var msg = Input.Text.Trim();
		Input.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;
		
		Say( msg );

		Canvas.TryScrollToBottom();
	}

	public void AddEntry( string name, string message, string avatar )
	{
		var e = Canvas.AddChild<ClassicChatEntry>();
		e.Message.Text = message;
		e.NameLabel.Text = name;
		e.Avatar.SetTexture( avatar );

		e.SetClass( "noname", string.IsNullOrEmpty( name ) );
		e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

		Canvas.TryScrollToBottom();
	}

	[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string avatar = null )
	{
		Current?.AddEntry( name, message, avatar );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ClientCmd( "chat_addinfo", CanBeCalledFromServer = true )]
	public static void AddInformation( string message, string avatar = null )
	{
		Current?.AddEntry( null, message, avatar );
	}

	[ServerCmd( "say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
	}
}
