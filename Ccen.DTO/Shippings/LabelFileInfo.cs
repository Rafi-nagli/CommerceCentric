namespace Amazon.DTO.Shippings
{
    public class LabelFileInfo
    {
        public string AbsoluteFilePath { get; set; }
        public string RelativeFilePath { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public string Unit { get; set; }
    }
}
