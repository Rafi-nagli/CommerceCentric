using System;

namespace Amazon.DTO
{
    public class LabelPrintPackDTO
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public int LabelsNumber { get; set; }
        public string SinglePackPersonName { get; set; }
    }
}
