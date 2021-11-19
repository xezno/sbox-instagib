using System;
using System.Linq;
using Sandbox;

namespace Instagib.GameStates
{
	public class BaseGameState
	{
		protected RealTimeSince stateStart;

		public BaseGameState()
		{
			stateStart = 0;
		}

		public virtual string StateName() => GetType().ToString();

		public virtual string StateTime()
		{
			var timeEndSpan = TimeSpan.FromSeconds( stateStart );
			var minutes = timeEndSpan.Minutes;
			var seconds = timeEndSpan.Seconds;
			return $"{minutes:D2}:{seconds:D2}";
		}

		public virtual void Tick() { }

		protected void SetState( BaseGameState newState )
		{
			(Game.Current as Game).CurrentState = newState;
		}

		protected int GetPlayerCount() => Client.All.Count();

		public virtual void OnKill( Client attackerClient, Client victimClient ) { }

		public virtual void OnDeath( Client cl ) { }

		public virtual void OnPlayerJoin( Client cl ) { }

		public virtual void OnPlayerLeave( Client cl ) { }
	}
}
