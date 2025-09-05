namespace DbModelGenerator.Models
{
    public class ForeignKeyInfo
    {
        public string FKColumn { get; set; } = string.Empty;
        public string PKColumn { get; set; } = string.Empty;
        public string FKTable { get; set; } = string.Empty;
        public string PKTable { get; set; } = string.Empty;
        public bool IsNullable { get; set; } = false;
    }

}
