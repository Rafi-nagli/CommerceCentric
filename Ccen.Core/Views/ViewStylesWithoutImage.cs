using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewStylesWithoutImage
    {
        [Key]
        public long Id { get; set; }
        public string StyleID { get; set; }
    }
}
