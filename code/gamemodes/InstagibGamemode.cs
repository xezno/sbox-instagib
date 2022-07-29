﻿namespace OpenArena;

[Library( "oa_gamemode_instagib" )]
public class InstagibGamemode : BaseGamemode
{
	public InstagibGamemode() : base() { }

	public override void RespawnPlayer( Player player )
	{
		base.RespawnPlayer( player );

		player.Health = 1; // One shot, one kill
	}

	protected override void SetInventory( Player player )
	{
		player.ActiveChild = new Railgun()
		{
			Owner = player
		};
	}

	protected override void CheckRespawning()
	{
		foreach ( var player in Players )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				if ( player.TimeSinceDied > 3 && Host.IsServer )
				{
					RespawnPlayer( player );
				}
			}
		}
	}
}