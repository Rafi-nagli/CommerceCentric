
namespace Amazon.Core.Models
{
    public enum AddressValidationStatus
    {
        None = 0,
        Valid = 1,
        ValidWithStreetWarnAndMultipleCandidate = 5,
        ValidWithStreetWarnAndNoCandidate = 10,
        
        Invalid = 100,
        InvalidCleanseHash = 101,

        Exception = 500,
        InvalidRecipientName = 501,
        MissingPhoneNumber = 505,
        DhlAddressLengthExceeded = 510,

        ExceptionCommunication = 503,
    }
}
