using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewListParent
    {
        [Key]
        public int Id { get; set; }

        public string ImageSource { get; set; }
        public string ProductName { get; set; }
        public string AmazonName { get; set; }
        public string StyleId { get; set; }
        public string ASIN { get; set; }
        public bool IsManualStyle { get; set; }
        public Guid? UniqueIdentifier { get; set; }

    }
}
