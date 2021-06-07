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
		
			// Tests:
			// new Medal( "wasdasdwasdsd", "", (_, _) => true, 0 ),
			// new Medal( "3756547", "", (_, _) => true, 0 ),
			// new Medal( "123213123", "", (_, _) => true, 0 ),
			// new Medal( "wasdad weee", "", (_, _) => true, 0 ),
			
			new Medal( "First Blood", "Get the first frag", (_, _) => false, 10 ),
			new Medal( "Revenge", "Frag the player who fragged you 3 times in a row", (_, _) => false, 10 ),
			new Medal( "Double Whammy", "Hit 2 enemies with one Railgun shot", (_, _) => false, 50 ),

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
			
			new Medal( "Excellent", "Frag 2 enemies in 3 seconds or less", (_, _) => false, 10 ),
			new Medal( "Triple Kill", "Frag 3 enemies with 3 seconds or less between each frag", (_, _) => false, 10 ),
			new Medal( "Buzzkill", "Frag a player on a streak", (_, victim) => victim.CurrentStreak > 3, 10 ),
			new Medal( "Fighter", "Frag 5 enemies with 5 seconds or less between each frag", (_, _) => false, 10 ),
			new Medal( "Avatar of Death", "Get 10 frags with 3 seconds or less between each frag", (_, _) => false, 1000 ),
			
			new Medal( "From the Grave", "Get a frag after you're dead", (_, _) => false, 10 ),
			new Medal( "Impressive", "Get two consecutive hits with the Railgun", (_, _) => false, 10 ),
			new Medal( "Kamikaze", "Kill yourself, but take an enemy with you", (_, _) => false, 0 ),
		};
		
		// TODO
		public static Medal[] DamageMedals = new Medal[] 
		{
			new Medal( "Net Master", "Deal 1000 damage without dying", (_, _) => false, 50 ),
			new Medal( "Damage Dealer", "Deal 5000 damage during the match", (_, _) => false, 25 ) 
		};
		
		// TODO
		public static Medal[] MatchMedals = new Medal[]
		{
			new Medal( "Gold", "Get 1st place after a match", (_, _) => false, 100 ),
			new Medal( "Silver", "Get 1st place after a match", (_, _) => false, 50 ),
			new Medal( "Bronze", "Get 1st place after a match", (_, _) => false, 25 ),
			new Medal( "Perfect", "Get 1st place after a match", (_, _) => false, 25 ),
			new Medal( "Match Complete", "Get 1st place after a match", (_, _) => false, 25 ),
			new Medal( "Most Accurate", "Get 1st place after a match", (_, _) => false, 50 ),
			new Medal( "Most Damage", "Get 1st place after a match", (_, _) => false, 50 ),
			new Medal( "Survivor", "Get 1st place after a match", (_, _) => false, 50 ),
		};
	}
}
