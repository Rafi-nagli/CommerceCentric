
namespace Amazon.DTO
{
    public class SizeDTO
    {
        public int Id { get; set; }
        public int? SizeGroupId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool DefaultIsChecked { get; set; }
        public string Departments { get; set; }

        //Additional
        public string SizeGroupName { get; set; }
    }
}
