using DbModelGenerator.Models;
using DbModelGenerator.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
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
                sb.AppendLine($"using Microsoft.EntityFrameworkCore;");
                sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
                sb.AppendLine($"using System.ComponentModel.DataAnnotations.Schema;");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"namespace {_namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    [Table(\"{table}\")]");
                sb.AppendLine($"    public class {typeName}");
                sb.AppendLine("    {");

                foreach (var Column in columns)
                {
                    string csType = TypeMapper.SqlTypeToCSharpType(Column.Type, Column.IsNullable);
                    string propertyName = NameEditor.PascalCase(Column.Name);
                    if (Column.Name.Equals("id", StringComparison.OrdinalIgnoreCase) || Column.Name.Equals($"{dbSetName}Id", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine("        [Key]");
                    }
                    else if (Column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"        [ForeignKey(\"{Column.Name[..^2]}\")]");
                    }
                    if (Column.IsNullable && !csType.EndsWith("?"))
                    {
                        csType += "?";
                    }
                    else if (!Column.IsNullable &&  == "string")
                    {
                        sb.AppendLine("        [Required]");
                        if (Column.TypeLength == -1)
                        {
                            Column.TypeLength = 4000;
                            sb.AppendLine($"        [MaxLength({Column.TypeLength})]");
                        }
                        else if (Column.TypeLength > 0)
                        {
                            sb.AppendLine($"        [MaxLength({Column.TypeLength})]");
                        }
                    }
                    if (csType == "date" || csType == "datetime")
                    {
                        sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }} = DateTime.Now; \n");

                    }
                    else
                    {
                        sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                    }
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
                        string IsNullableMark = fk.IsNullable ? "?" : "";
                        sb.AppendLine($"        public {relatedType}{IsNullableMark} {fkColumnName} {{ get; set; }}");
                    }
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                SaveFile(path, sb.ToString(), $"{typeName}.cs");
            }
        }
        catch (Exception ex)
        {
            string message = $"Table Model Creation Error: {ex.Message}";
            _getSql.ErrorLog(message);
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
                            string csType = TypeMapper.SqlTypeToCSharpType(col.Type, col.IsNullable);
                            string propertyName = NameEditor.PascalCase(col.Name);
                            if (col.IsNullable && !csType.EndsWith("?"))
                            {
                                csType += "?";
                            }
                            sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                        }
                        sb.AppendLine("    }");
                        var parameters = _getSql.GetProcedureParameters(prosedur);
                        if (parameters.Any())
                        {
                            var paramClassName = NameEditor.PascalCase(prosedur) + "Params";

                            sb.AppendLine($"    public class {paramClassName}");
                            sb.AppendLine("    {");

                            foreach (var param in parameters)
                            {
                                string csType = TypeMapper.SqlTypeToCSharpType(param.Type, param.IsNullable);
                                string propertyName = NameEditor.PascalCase(param.Name.TrimStart('@'));
                                if (param.IsNullable && !csType.EndsWith("?"))
                                {
                                    csType += "?";
                                }
                                sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}\n");
                            }
                            sb.AppendLine("    }");
                            sb.AppendLine("}");
                        }
                        

                        SaveFile(path, sb.ToString(), $"{ClassName}.cs");
                    }
                    catch (Exception ex)
                    {
                        string message = $"Procedure Model Error ({prosedur}): {ex.Message}";
                        _getSql.ErrorLog(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string message = $"Procedure Model Creation Error: {ex.Message}";
            _getSql.ErrorLog(message);
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
            sb.AppendLine("     {");
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

                foreach (var unique in _getSql.GetUniqueColmun(table))
                {
                    string entityName = EnglishInflector.ToSingular(NameEditor.PascalCase(unique.TableName));
                    string colName = NameEditor.PascalCase(unique.ColmunName);

                    sb.AppendLine($"            modelBuilder.Entity<{entityName}>()");
                    sb.AppendLine($"                .HasIndex(e => e.{colName})");
                    sb.AppendLine($"                .IsUnique();");
                    sb.AppendLine();
                }
            }

            foreach (var fk in _getSql.GetForeignKeys())
            {
                string fkEntity = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.FKTable));
                string pkEntity = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.PKTable));
                string fkPropAbbr = NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.FKTable));
                string pkPropAbbr = NameEditor.GetAbbreviation(NameEditor.PascalCase(fk.PKTable));
                string deleteBehavior = fk.IsNullable ? "DeleteBehavior.SetNull" : "DeleteBehavior.Cascade";

                sb.AppendLine($"                modelBuilder.Entity<{fkEntity}>()");
                sb.AppendLine($"                    .HasOne({fkPropAbbr} => {fkPropAbbr}.{pkEntity})");
                sb.AppendLine($"                    .WithMany({pkPropAbbr} => {pkPropAbbr}.{fk.FKTable})");
                sb.AppendLine($"                    .HasForeignKey({fkPropAbbr} => {fkPropAbbr}.{NameEditor.PascalCase(fk.FKColumn)})");
                sb.AppendLine($"                    .OnDelete({deleteBehavior})");
                sb.AppendLine($"                    .HasConstraintName(\"FK_{fk.FKTable}_{fk.PKTable}\");");
            }
            sb.AppendLine("         }");
            sb.AppendLine("     }");
            sb.AppendLine("}");

            SaveFile(path, sb.ToString(), "AppDbContext.cs");
        }
        catch (Exception ex)
        {
            string message = $"DbContext Creation Error: {ex.Message}";
            _getSql.ErrorLog(message);
        }
    }

    public void GenerateAll(string path, string _namespace)
    {
        tableModelGenerator(path, _namespace);
        ProsedurModelGenerator(path, _namespace);
        DBContextGenerator(path, _namespace);
    }

    private void SaveFile(string path, string content, string fileName = "AppDbContext.cs")
    {
        try
        {
            File.WriteAllText(Path.Combine(path, fileName), content);
            Console.WriteLine($"{fileName} created.");
        }
        catch (Exception ex)
        {
            string message = $"File Save Error ({fileName}): {ex.Message}";
            _getSql.ErrorLog(message);
        }
    }
}
