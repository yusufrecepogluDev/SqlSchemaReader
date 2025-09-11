namespace DbModelGenerator.Utils
{
    internal class TypeMapper
    {
        public static string SqlTypeiToCSharpType(string sqlTypei, bool IsNullable)
        {
            string csharpType = sqlTypei switch
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

            if (csharpType != "string" && csharpType != "byte[]" && csharpType != "object" && IsNullable)
            {
                return csharpType + "?";
            }
            return csharpType;
        }

    }
}
