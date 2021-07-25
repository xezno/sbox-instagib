using Sandbox.Html;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.UI
{
	public class PopupButton : Button
	{
		protected Popup Popup;

		public PopupButton()
		{
			AddClass( "popupbutton" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			Open();
		}

		public virtual void Open()
		{
			Popup = new Popup( this, Popup.PositionMode.BelowCenter, 4.0f );
			Popup.AddOption( "Override Me!" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Popup != null && !Popup.IsDeleting );

			if ( Popup  != null )
			{
				Popup.Style.Width = Box.Rect.width;
			}
		}
	}
	public class Option
	{
		public string Title;
		public string Icon;
		public string Subtitle;
		public string Tooltip;
		public object Value;
	}

	[Library( "select" )]
	public class DropDown : PopupButton
	{
		protected IconPanel DropdownIndicator;

		public List<Option> Options { get; } = new();

		public DropDown()
		{
			AddClass( "dropdown" );
			DropdownIndicator = Add.Icon( "expand_more", "dropdown_indicator" );
		}

		public override void SetProperty( string name, string value )
		{
			base.SetProperty( name, value );

			if ( name == "value" )
			{
				Option option = Options.FirstOrDefault( x => x.Value.ToString() == value );
				if ( option != null )
				{
					Selected = option;
				}
				else
				{
					Selected = null;
					Text = value;
				}
			}
		}

		public override void Open()
		{
			Popup = new Popup( this, Popup.PositionMode.BelowCenter, 4.0f );
			Popup.AddClass( "flat-top" );

			foreach( var option in Options )
			{
				Popup.AddOption( option.Title, () => Select( option ) );
			}
		}

		protected virtual void Select( Option option )
		{
			Selected = option;
		}

		Option selected;

		public Option Selected 
		{
			get => selected;
			set
			{
				if ( selected == value ) return;
				selected = value;

				if ( selected != null )
				{
					Text = selected.Title;
					Icon = selected.Icon;
					CreateValueEvent( "value", selected?.Value );
				}
			}
		}

		public override bool OnTemplateElement( INode element )
		{
			Options.Clear();

			foreach ( var child in element.Children )
			{
				if ( !child.IsElement ) continue;

				if ( child.Name.Equals( "option", StringComparison.OrdinalIgnoreCase ) )
				{
					var o = new Option();

					o.Title = child.InnerHtml;
					o.Value = child.GetAttributeValue( "value", child.InnerHtml );

					Options.Add( o );
				}
			}

			return true;
		}
	}
}
