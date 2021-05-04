using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.ViewModels.PopupResults
{
    public class OpenUrlResult : MessageResult
    {
        public string Url { get; set; }
    }
}