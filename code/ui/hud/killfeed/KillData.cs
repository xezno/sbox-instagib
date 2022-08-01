namespace Instagib;

public struct KillData
{
	public long AttackerPlayerID { get; set; }
	public string AttackerName { get; set; }
	public long VictimPlayerID { get; set; }
	public string VictimName { get; set; }
	public string Method { get; set; }

	public KillData( long attackerPlayerID, string attackerName, long victimPlayerID, string victimName, string method )
	{
		AttackerPlayerID = attackerPlayerID;
		AttackerName = attackerName;
		VictimPlayerID = victimPlayerID;
		VictimName = victimName;
		Method = method;
	}
}
