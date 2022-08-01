namespace Instagib;

public static class ClientExtensions
{
	public static int GetPlacement( this Client client )
	{
		var orderedClients = OrderClients();
		var index = orderedClients.Select( ( v, i ) => new { client = v, index = i } ).First( x => x.client == client ).index;

		return index + 1;
	}

	public static IEnumerable<Client> OrderClients()
	{
		return Client.All.OrderBy( x => (-x.GetInt( "kills" ) * 1000) + x.GetInt( "deaths" ) );
	}
}
