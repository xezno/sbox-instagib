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

		public delegate bool ConditionDelegate( Player attacker, Player victim );
		
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
			new Medal( "Headshot",
				"Shoot someone in the head",
				( attacker, victim) => victim.LastHitboxDamaged == Player.HitboxGroup.Head,
				50 ),
			new Medal( "Nutshot",
				"Shoot someone in the goolies",
				( attacker, victim) => victim.LastHitboxDamaged == Player.HitboxGroup.Stomach,
				10 ),
		};
	}
}
