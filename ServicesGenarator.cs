using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModelGenerator
{
    internal class ServicesGenarator
    {

        public static string GenerateServiceClass(string className, string namespaceName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine($"using {namespaceName}.Models;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Services");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}Service");
            sb.AppendLine("    {");
            sb.AppendLine("         private readonly string _connectionString;");
            sb.AppendLine();
            sb.AppendLine($"        public {className}Service(string connectionString)");
            sb.AppendLine("        {");
            sb.AppendLine("            _connectionString = connectionString;");
            sb.AppendLine("         }");
            sb.AppendLine();
            sb.AppendLine($"         public List<{className}> GetAll()");
            sb.AppendLine("         {");
            sb.AppendLine($"            var list = new List<{className}>();");
            sb.AppendLine($"            using var conn = new SqlConnection(_connectionString);");
            sb.AppendLine("             conn.Open();");
            sb.AppendLine("             // Implement data retrieval logic here");
            sb.AppendLine($"             return list;");
            sb.AppendLine("         }");
            sb.AppendLine();
            sb.AppendLine($"        public void Add({className} {className.ToLowerInvariant()})");
            sb.AppendLine("         {");
            sb.AppendLine($"            using var conn = new SqlConnection(_connectionString);");
            sb.AppendLine("             conn.Open();");
            sb.AppendLine("             // Implement data insertion logic here");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public void Update({className} {className.ToLowerInvariant()})");
            sb.AppendLine("         {");
            sb.AppendLine($"            using var conn = new SqlConnection(_connectionString);");
            sb.AppendLine("             conn.Open();");
            sb.AppendLine("             // Implement data insertion logic here");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public void Delete({className} {className.ToLowerInvariant()})");
            sb.AppendLine("         {");
            sb.AppendLine($"            using var conn = new SqlConnection(_connectionString);");
            sb.AppendLine("             conn.Open();");
            sb.AppendLine("             // Implement data insertion logic here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

    }
}
