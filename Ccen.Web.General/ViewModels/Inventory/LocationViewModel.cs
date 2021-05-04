
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Inventory
{
    public class LocationViewModel
    {
        public long Id { get; set; }
        public string Isle { get; set; }
        public string Section { get; set; }
        public string Shelf { get; set; }
        public bool IsDefault { get; set; }
        
        public bool IsEmpty()
        {
            return (string.IsNullOrEmpty(Isle) && string.IsNullOrEmpty(Section) && string.IsNullOrEmpty(Shelf))
                || (Isle == "0" && Section == "0" && Shelf == "0");
        }

        public LocationViewModel()
        {
            
        }

        public LocationViewModel(StyleLocationDTO location)
        {
            Id = location.Id;
            Isle = location.Isle;
            Section = location.Section;
            Shelf = location.Shelf;
            IsDefault = location.IsDefault;
        }
    }
}