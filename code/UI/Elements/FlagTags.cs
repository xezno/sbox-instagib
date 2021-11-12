using Instagib.Entities;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagib.UI.Elements
{
	public class FlagTags : Panel
	{
		private Dictionary<FlagEntity, Label> Tags { get; set; } = new();
		public FlagTags()
		{
			StyleSheet.Load( "/Code/UI/Elements/FlagTags.scss" );
			foreach ( var flagEntity in Entity.All.Where( e => e is FlagEntity ) )
			{
				var flagTeam = (flagEntity as FlagEntity).Team;
				var tag = Add.Icon( "flag", "flag-tag " + flagTeam.TeamName );

				Tags.Add( flagEntity as FlagEntity, tag );
			}
		}

		public override void Tick()
		{
			base.Tick();

			foreach ( var tagPair in Tags )
			{
				var entity = tagPair.Key;
				var panel = tagPair.Value;

				var offset = new Vector3( 0, 0, 100 );

				var screenPos = (entity.Position + offset).ToScreen();

				panel.Style.Left = Length.Fraction( screenPos.x );
				panel.Style.Top = Length.Fraction( screenPos.y );
			}
		}
	}
}
