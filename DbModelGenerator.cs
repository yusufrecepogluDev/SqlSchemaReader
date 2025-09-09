using DbModelGenerator.Models;
using DbModelGenerator.Utils;
using Microsoft.Data.SqlClient;
using System.Text;

namespace DbModelGenerator;
public class DbModelGenerator
{
    private GetSql _getSql;

    public DbModelGenerator(string server, string database)
    {
        _getSql = new GetSql(server, database);
    }

    public void tableModelGenerator(string path, string _namespace)
    {
        try
        {
            foreach (var table in _getSql.GetTables())
            {
                List<Column> columns = _getSql.GetColumns(table);
                var sb = new StringBuilder();
                var className = NameEditor.PascalCase(table);
                var dbSetName = className.Length > 1 ? className[..^1] : className;

                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(table));
                sb.AppendLine($"namespace {_namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public class {typeName}");
                sb.AppendLine("    {");

                foreach (var Column in columns)
                {
                    string csType = TypeMapper.SqlTypeiToCSharpType(Column.Type, Column.Nullable);
                    string propertyName = NameEditor.PascalCase(Column.Name);
                    if (Column.Nullable && !csType.EndsWith("?"))
                    {
                        csType += "?";
                    }
                    sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                }
                foreach (var fk in _getSql.GetForeignKeys().Where(fk => fk.FKTable == table || fk.PKTable == table))
                {
                    bool isCollection = fk.PKTable == table; 
                    string relatedType = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.PKTable == table ? fk.FKTable : fk.PKTable));
                    string fkColumnName = NameEditor.PascalCase(fk.FKColumn).Replace("Id", "");

                    if (isCollection)
                    {
                        string propertyName = EnglishInflector.ToPlural(relatedType);
                        sb.AppendLine($"        public List<{relatedType}> {propertyName} {{ get; set; }} = new List<{relatedType}>();");
                    }
                    else
                    {
                        string nullableMark = fk.IsNullable ? "?" : "";
                        sb.AppendLine($"        public {relatedType}{nullableMark} {fkColumnName} {{ get; set; }}");
                    }
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");
                string fileName = Path.Combine(path, $"{NameEditor.PascalCase(table)}.cs");
                File.WriteAllText(fileName, sb.ToString());
                Console.WriteLine($"{fileName} created.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Table Model Creation Error: {ex.Message}");
        }
    }

    public void ProsedurModelGenerator(string path, string _namespace)
    {
        try
        {
            foreach (var prosedur in _getSql.GetProcedures())
            {
                var resultColumns = _getSql.GetProcedureColumns(prosedur);
                var ClassName = NameEditor.PascalCase(prosedur);
                if (resultColumns.Any())
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"namespace {_namespace}");
                        sb.AppendLine("{");
                        sb.AppendLine($"    public class {ClassName}");
                        sb.AppendLine("    {");

                        foreach (var col in resultColumns)
                        {
                            string csType = TypeMapper.SqlTypeiToCSharpType(col.Type, col.Nullable);
                            string propertyName = NameEditor.PascalCase(col.Name);
                            if (col.Nullable && !csType.EndsWith("?"))
                            {
                                csType += "?";
                            }
                            sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                        }

                        sb.AppendLine("    }");
                        sb.AppendLine("}");

                        string fileName = Path.Combine(path, $"{ClassName}.cs");
                        File.WriteAllText(fileName, sb.ToString());
                        Console.WriteLine($"{fileName} created.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Procedure Column Model Error ({prosedur}): {ex.Message}");
                    }
                }
                // parameter modeli
                var parameters = _getSql.GetProcedureParameters(prosedur);
                if (parameters.Any())
                {
                    try
                    {
                        var sb = new StringBuilder();
                        var paramClassName = NameEditor.PascalCase(prosedur) + "Params";

                        sb.AppendLine($"namespace {_namespace}");
                        sb.AppendLine("{");
                        sb.AppendLine($"    public class {paramClassName}");
                        sb.AppendLine("    {");

                        foreach (var param in parameters)
                        {
                            string csType = TypeMapper.SqlTypeiToCSharpType(param.Type, param.Nullable);
                            string propertyName = NameEditor.PascalCase(param.Name.TrimStart('@'));
                            if (param.Nullable && !csType.EndsWith("?"))
                            {
                                csType += "?";
                            }
                            sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                        }

                        sb.AppendLine("    }");
                        sb.AppendLine("}");

                        string fileName = Path.Combine(path, $"{ClassName}Params.cs");
                        File.WriteAllText(fileName, sb.ToString());
                        Console.WriteLine($"{fileName} created.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Procedure Parameter Model Error ({prosedur}): {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Procedure Model Creation Error: {ex.Message}");
        }
    }

    public void DBContextGenerator(string path, string _namespace)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public class AppDbContext : DbContext");
            sb.AppendLine("    {");
            sb.AppendLine("        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }\n");

            foreach (var table in _getSql.GetTables())
            {
                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(table));
                string propName = EnglishInflector.ToPlural(typeName);
                sb.AppendLine($"        public DbSet<{typeName}> {propName} {{ get; set; }}\n");
            }
            sb.AppendLine("    ");
            sb.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");
            foreach (var table in _getSql.GetTables())
            {
                sb.AppendLine($"            modelBuilder.Entity<{EnglishInflector.ToSingular(NameEditor.PascalCase(table))}>().ToTable(\"{table}\");\n");
            }
            sb.AppendLine("        ");
            sb.AppendLine("        ");
            foreach (var fk in _getSql.GetForeignKeys())
            {
                sb.AppendLine($"            modelBuilder.Entity<{EnglishInflector.ToSingular(NameEditor.PascalCase(fk.FKTable))}>()");
                sb.AppendLine($"                .HasOne({NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.FKTable))} => {NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.FKTable))}.{EnglishInflector.ToSingular(fk.PKTable)})");
                sb.AppendLine($"                .WithMany({NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.PKTable))} => {NameEditor.GetAbbreviation(NameEditor.      PascalCase(fk.PKTable))}.{fk.FKTable})");
                sb.AppendLine($"                .HasForeignKey({NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.FKTable))} =>  {NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.FKTable))}.{NameEditor.PascalCase(fk.FKColumn)})");
                sb.AppendLine($"                .HasConstraintName(\"FK_{fk.FKTable}_{fk.PKTable}\");\n");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            string fileName = Path.Combine(path, "AppDbContext.cs");
            File.WriteAllText(fileName, sb.ToString());
            Console.WriteLine($"{fileName} created.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DbContext Creation Error: {ex.Message}");
        }
    }
}
