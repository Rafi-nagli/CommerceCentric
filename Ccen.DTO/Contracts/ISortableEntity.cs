
namespace Amazon.DTO.Contracts
{
    public interface ISortableEntity : ISortableByName, ISortableByLocation, ISortableByShippingMethod
    {
        long Id { get; }
    }
}
