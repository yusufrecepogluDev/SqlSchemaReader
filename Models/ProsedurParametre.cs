namespace DbModelGenerator.Models
{
    public class Prosedurparameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsOutput { get; set; }
    }

}
