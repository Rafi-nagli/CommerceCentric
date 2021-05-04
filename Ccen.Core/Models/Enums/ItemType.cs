using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Enums
{
    public class ItemType
    {
        //PA
        public const int Pajama = 1;
        public const int Robe = 2;
        public const int Umbrella = 3;
        public const int Hat = 4;
        public const int Underwear = 5;
        public const int Costume = 6;
        public const int Accessory = 7;
        public const int Towel = 8;
        public const int Shoes = 9;

        //DWS
        public const int Watches = 1;
        public const int Sunglasses = 2;
        public const int Jawelry = 3;
        public const int WritingInstrument = 10;


        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
