public static class ObjectExtensions
{
	public static string GetLibraryName( this object obj )
	{
		return TypeLibrary.GetType( obj.GetType() )?.GetAttribute<LibraryAttribute>()?.Name ?? "none";
	}
}
