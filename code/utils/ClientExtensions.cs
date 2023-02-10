namespace Instagib;

public static class ClientExtensions
{
	public static int GetPlacement( this IClient client )
	{
		var orderedClients = OrderClients();
		var index = orderedClients.Select( ( v, i ) => new { client = v, index = i } ).First( x => x.client == client ).index;

		return index + 1;
	}

	public static IEnumerable<IClient> OrderClients()
	{
		return Game.Clients.OrderBy( x => (-x.GetInt( "kills" ) * 1000) + x.GetInt( "deaths" ) );
	}
}
