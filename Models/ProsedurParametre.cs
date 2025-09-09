namespace DbModelGenerator.Models
{
    public class Prosedurparameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Nullable { get; set; }
        public bool IsOutput { get; set; }
    }

}
