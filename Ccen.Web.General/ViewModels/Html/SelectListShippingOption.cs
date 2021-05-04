namespace Amazon.Web.ViewModels.Html
{
    public class SelectListShippingOption : SelectListItemTag
    {
        public bool RequiredPackageSize { get; set; }
        public int PackageCount { get; set; }
    }
}