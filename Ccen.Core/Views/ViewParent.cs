using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewParent
    {
        [Key]
        public int Id { get; set; }

        public string ImageSource { get; set; }
        public string ProductName { get; set; }
        public string AmazonName { get; set; }
        public string StyleId { get; set; }
        public string ASIN { get; set; }

        public int SubLicenseId { get; set; }
        public int SecondarySubLicenseId { get; set; }
        public int Gender { get; set; }
        public Guid? UniqueIdentifier { get; set; }
    }
}
