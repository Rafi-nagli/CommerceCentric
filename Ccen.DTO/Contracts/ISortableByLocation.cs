
namespace Amazon.DTO.Contracts
{
    public interface ISortableByLocation
    {
        int SortIsle { get; }
        int SortSection { get; }
        int SortShelf { get; }

        //long? SortLocationIndex { get; }

        string SortStyleString { get; }
        string SortSize { get; }
        string SortColor { get; }
        string FirstItemName { get; }
        string SortOrderId { get; }
    }
}
