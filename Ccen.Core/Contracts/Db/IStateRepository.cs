using Amazon.Core.Entities;

namespace Amazon.Core.Contracts.Db
{
    public interface IStateRepository : IRepository<State>
    {
        string GetCodeByName(string shippingState);
    }
}
