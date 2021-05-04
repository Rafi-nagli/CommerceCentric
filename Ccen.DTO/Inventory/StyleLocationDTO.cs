
namespace Amazon.DTO
{
    public class StyleLocationDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int SortIsle { get; set; }
        public int SortSection { get; set; }
        public int SortShelf { get; set; }

        public string Isle { get; set; }
        public string Section { get; set; }
        public string Shelf { get; set; }

        public bool IsDefault { get; set; }
    }
}
