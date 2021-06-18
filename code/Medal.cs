using System;

namespace Instagib
{
	public struct Medal
	{
		public string Name { get; set; }
		public string Description { get; set; }
		
		/// <summary>
		/// Attacker, Victim
		/// <para>
		/// Return true if medal is awarded.
		/// </para>
		/// </summary>
		public ConditionDelegate Condition { get; set; }
		public int Experience { get; set; }

		public delegate bool ConditionDelegate( InstagibPlayer attacker, InstagibPlayer victim );
		
		public Medal( string name, string description, ConditionDelegate condition, int experience )
		{
			Name = name;
			Description = description;
			Condition = condition;
			Experience = experience;
		}
	}

	public static class Medals
	{
		public static Medal[] KillMedals = new Medal[]
		{
			new Medal( "Airborne",
				"Frag an enemy that is in the air",
				( attacker, victim ) =>
				{
					return (victim.GroundEntity == null);
				},
				50 ),

			new Medal( "Killing Spree",
				"Get 5 frags without dying",
				( attacker, _ ) => attacker.CurrentStreak == 5,
				10 ),
			new Medal( "Rage",
				"Get 10 frags without dying",
				( attacker, _ ) => attacker.CurrentStreak == 10,
				10 ),
			new Medal( "Frenzy",
				"Get 20 frags without dying",
				( attacker, _ ) => attacker.CurrentStreak == 20,
				10 ),
			new Medal( "Rampage",
				"Get 30 frags without dying",
				( attacker, _ ) => attacker.CurrentStreak == 30,
				10 ),
			
			// TODO: All of below
			new Medal( "Excellent", "Frag 2 enemies in 3 seconds or less", (_, _) => false, 10 ),
			new Medal( "Triple Kill", "Frag 3 enemies with 3 seconds or less between each frag", (_, _) => false, 10 ),
			new Medal( "Buzzkill", "Frag a player on a streak", (_, victim) => victim.CurrentStreak > 3, 10 ),
			new Medal( "Fighter", "Frag 5 enemies with 5 seconds or less between each frag", (_, _) => false, 10 ),
			new Medal( "Avatar of Death", "Get 10 frags with 3 seconds or less between each frag", (_, _) => false, 1000 ),
			
			new Medal( "From the Grave", "Get a frag after you're dead", (_, _) => false, 10 ),
			new Medal( "Impressive", "Get two consecutive hits with the Railgun", (_, _) => false, 10 ),
			new Medal( "Kamikaze", "Kill yourself, but take an enemy with you", (_, _) => false, 0 ),
		};
	}
}
