using Sandbox;
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
		public static Medal[] KillMedals => new Medal[]
		{
			new Medal( "Airborne",
				( attacker, victim ) =>
				{
					return (victim.GroundEntity == null);
				}),

			new Medal( "Longshot",
				( attacker, victim ) =>
				{
					var distance = attacker.Position.Distance( victim.Position );
					Log.Trace( distance );
					return ( distance > 2048 );
				}),

			new Medal( "Up Close",
				( attacker, victim ) =>
				{
					var distance = attacker.Position.Distance( victim.Position );
					Log.Trace( distance );
					return ( distance < 128 );
				}),

			new Medal( "Killing Spree (5 💀)",
				( attacker, _ ) => attacker.CurrentStreak == 5),

			new Medal( "Frenzy (10 💀)",
				( attacker, _ ) => attacker.CurrentStreak == 10),

			new Medal( "Rampage (20 💀)",
				( attacker, _ ) => attacker.CurrentStreak == 20),

			new Medal( "Unstoppable (30 💀)",
				( attacker, _ ) => attacker.CurrentStreak == 30),

			new Medal( "Buzzkill",
				(_, victim) => victim.CurrentStreak > 3),
		};
	}
}
