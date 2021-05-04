using Amazon.DTO;

namespace Amazon.Core.Contracts.Parser
{
    public interface ILineParser
    {
        IReportItemDTO Parse(string[] fields, string[] headers);
    }
}
