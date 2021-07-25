using System;
using Instagib.UI.Elements;
using Instagib.Utils;
using Sandbox;
using Sandbox.UI;

namespace Instagib.UI.Menus
{
	[UseTemplate]
	public class ProfileMenu : BaseMenu
	{
		public Panel PlayerAvatar { get; set; }
		public Label PlayerName { get; set; }
		
		public Label PlayerTitle { get; set; }
		
		private PlayerStats PlayerStats { get; set; }

		public Label DeathsLabel { get; set; }
		public Label KillsLabel { get; set; }
		public Label KdrLabel { get; set; }
		
		public Label TimePlayedLabel { get; set; }
		public Label DateJoinedLabel { get; set; }
		
		public Panel Scroll { get; set; }
		
		public ProfileMenu()
		{
			StyleSheet.Load( "/Code/UI/Menus/ProfileMenu.scss" );
			
			PlayerStats = Stats.Instance.RequestStats();
			PlayerAvatar.Style.SetBackgroundImage( $"avatar:{Local.SteamId}" );
			PlayerAvatar.Style.Dirty();

			PlayerName.Text = Local.DisplayName;
			PlayerTitle.Text = "Instagibsta";

			DeathsLabel.Text = PlayerStats.Deaths.ToString();
			KillsLabel.Text = PlayerStats.Kills.ToString();
			KdrLabel.Text = PlayerStats.Kdr.ToString("f1");

			var timePlayed = TimeSpan.FromSeconds( PlayerStats.TimePlayed );
			var hours = timePlayed.Hours + (timePlayed.Days * 24);
			var minutes = timePlayed.Minutes;
			TimePlayedLabel.Text = $"{hours}h {minutes}m {(hours > 50 ? "(get a life xx)" : "")}";
			
			DateJoinedLabel.Text = DateTimeOffset.FromUnixTimeMilliseconds( PlayerStats.TimeRegistered ).ToString( "d" );

			var scrollbar = AddChild<Scrollbar>();
			scrollbar.Panel = Scroll;
		}
	}
}
