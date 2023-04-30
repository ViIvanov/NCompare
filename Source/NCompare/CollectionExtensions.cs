namespace NCompare;

internal static class CollectionExtensions
{
  public static void Add<T>(this List<T> source, IEnumerable<T> items) {
    if(source is null) {
      throw new ArgumentNullException(nameof(source));
    }//if

    source.AddRange(items);
  }
}
