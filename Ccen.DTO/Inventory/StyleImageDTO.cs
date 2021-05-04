namespace Amazon.DTO.Inventory
{
    public class StyleImageDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int Type { get; set; }
        public string Image { get; set; }
        public string SourceImage { get; set; }
        public string SourceMarketId { get; set; }

        public int Category { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }

        public string Tags { get; set; }

        public long? OrderIndex { get; set; }
    }
}
