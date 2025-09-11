namespace DbModelGenerator.Models
{
    public class Column
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TypeLength { get; set; }
        public bool IsNullable { get; set; }
    }

}
