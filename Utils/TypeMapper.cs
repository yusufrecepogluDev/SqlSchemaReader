namespace DbModelGenerator.Utils
{
    internal class TypeMapper
    {
        public static string SqlTipiToCSharpTip(string sqlTipi, bool nullable)
        {
            string csharpTip = sqlTipi switch
            {
                "int" => "int",
                "bigint" => "long",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
                "float" => "double",
                "real" => "float",
                "date" or "datetime" or "smalldatetime" or "datetime2" => "DateTime",
                "datetimeoffset" => "DateTimeOffset",
                "uniqueidentifier" => "Guid",
                "time" => "TimeSpan",
                "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext" => "string",
                "xml" => "string",
                "binary" or "varbinary" or "image" or "rowversion" or "timestamp" => "byte[]",
                "sql_variant" => "object",
                _ => "string"
            };

            if (csharpTip != "string" && csharpTip != "byte[]" && csharpTip != "object" && nullable)
            {
                return csharpTip + "?";
            }
            return csharpTip;
        }

    }
}
