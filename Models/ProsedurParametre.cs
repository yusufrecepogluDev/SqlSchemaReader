namespace DbModelGenerator.Models
{
    public class ProsedurParametre
    {
        public string Ad { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public bool Nullable { get; set; }
        public bool IsOutput { get; set; }
    }

}
