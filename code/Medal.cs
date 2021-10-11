using System;

namespace Instagib
{
	public struct Medal
	{
		public string Name { get; set; }

		/// <summary>
		/// Attacker, Victim
		/// <para>
		/// Return true if medal is awarded.
		/// </para>
		/// </summary>
		public ConditionDelegate Condition { get; set; }

		public delegate bool ConditionDelegate( Player attacker, Player victim );

		public Medal( string name, ConditionDelegate condition )
		{
			Name = name;
			Condition = condition;
		}
	}

	public static class Medals
	{
		public static Medal[] KillMedals = new Medal[]
		{
			new Medal( "Airborne",
				( attacker, victim ) =>
				{
					return (victim.GroundEntity == null);
				}),

			new Medal( "Killing Spree",
				( attacker, _ ) => attacker.CurrentStreak == 5),

			new Medal( "Rage",
				( attacker, _ ) => attacker.CurrentStreak == 10),

			new Medal( "Frenzy",
				( attacker, _ ) => attacker.CurrentStreak == 20),

			new Medal( "Rampage",
				( attacker, _ ) => attacker.CurrentStreak == 30),

			new Medal( "Buzzkill",
				(_, victim) => victim.CurrentStreak > 3),
		};
	}
}
