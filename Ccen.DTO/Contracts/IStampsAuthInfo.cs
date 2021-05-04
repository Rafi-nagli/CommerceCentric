namespace Amazon.DTO.Contracts
{
    public interface IStampsAuthInfo
    {
        int AccountType { get; }
        string StampIntegration { get; }
        string StampUsername { get; }
        string StampPassword { get; }
    }
}
