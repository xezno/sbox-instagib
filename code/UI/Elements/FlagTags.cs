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
		}

		private void Update()
		{
			foreach ( var flagEntity in Entity.All.Where( e => e is FlagEntity ) )
			{
				var flag = flagEntity as FlagEntity;

				if ( Tags.ContainsKey( flag ) )
					continue;

				var tag = Add.Icon( "flag", "flag-tag " + flag.Team.TeamName );

				Tags.Add( flag, tag );
			}

			timeSinceLastUpdate = 0;
		}

		TimeSince timeSinceLastUpdate;

		public override void Tick()
		{
			base.Tick();

			foreach ( var tagPair in Tags )
			{
				var entity = tagPair.Key;
				var panel = tagPair.Value;

				if ( !entity.IsValid() )
				{
					panel.Delete();
					return;
				}

				var offset = new Vector3( 0, 0, 100 );

				var screenPos = (entity.Position + offset).ToScreen();

				panel.Style.Left = Length.Fraction( screenPos.x );
				panel.Style.Top = Length.Fraction( screenPos.y );
			}

			if ( timeSinceLastUpdate > 1 )
			{
				Update();
			}
		}
	}
}
