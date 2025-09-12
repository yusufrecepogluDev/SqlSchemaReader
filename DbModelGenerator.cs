using DbModelGenerator.Models;
using DbModelGenerator.Utils;
using Microsoft.EntityFrameworkCore;
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
                var typeName = EnglishInflector.ToSingular(className);

                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                sb.AppendLine();
                sb.AppendLine($"namespace {_namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    [Table(\"{table}\")]");
                sb.AppendLine($"    public class {typeName}");
                sb.AppendLine("    {");

                foreach (var column in columns)
                {
                    string csType = TypeMapper.SqlTypeToCSharpType(column.Type, column.IsNullable);
                    string propertyName = NameEditor.PascalCase(column.Name);

                    // Primary Key
                    if (column.Name.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                        column.Name.Equals($"{typeName}Id", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine("        [Key]");
                    }

                    // Required & MaxLength for strings
                    if (!column.IsNullable && csType == "string")
                    {
                        sb.AppendLine("        [Required]");
                    }
                    if (csType == "string" && column.TypeLength > 0)
                    {
                        int length = column.TypeLength == -1 ? 4000 : column.TypeLength;
                        sb.AppendLine($"        [MaxLength({length})]");
                    }

                    // Nullable type
                    if (column.IsNullable && !csType.EndsWith("?") && csType != "string")
                    {
                        csType += "?";
                    }

                    // Default value for DateTime
                    if ((csType == "DateTime" || csType == "DateTime?") && !column.IsNullable)
                    {
                        sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }} = DateTime.Now;");
                    }
                    else
                    {
                        sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}");
                    }

                    sb.AppendLine();
                }

                // Navigation properties
                foreach (var fk in _getSql.GetForeignKeys().Where(fk => fk.FKTable == table || fk.PKTable == table))
                {
                    bool isCollection = fk.PKTable == table;
                    string relatedType = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.PKTable == table ? fk.FKTable : fk.PKTable));

                    if (isCollection)
                    {
                        string propertyName = EnglishInflector.ToPlural(relatedType);
                        sb.AppendLine($"        public List<{relatedType}> {propertyName} {{ get; set; }} = new List<{relatedType}>();");
                    }
                    else
                    {
                        string fkPropName = NameEditor.PascalCase(fk.FKColumn);
                        if (fkPropName.EndsWith("Id")) fkPropName = fkPropName[..^2];
                        string nullableMark = fk.IsNullable ? "?" : "";
                        sb.AppendLine($"        public {relatedType}{nullableMark} {fkPropName} {{ get; set; }}");
                    }
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                SaveFile(path, sb.ToString(), $"{typeName}.cs");
            }
        }
        catch (Exception ex)
        {
            _getSql.ErrorLog($"Table Model Creation Error: {ex.Message}");
        }
    }

    public void ProsedurModelGenerator(string path, string _namespace)
    {
        try
        {
            foreach (var prosedur in _getSql.GetProcedures())
            {
                var resultColumns = _getSql.GetProcedureColumns(prosedur);
                if (!resultColumns.Any()) continue;

                var sb = new StringBuilder();
                var className = NameEditor.PascalCase(prosedur);

                sb.AppendLine($"namespace {_namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public class {className}");
                sb.AppendLine("    {");

                foreach (var col in resultColumns)
                {
                    string csType = TypeMapper.SqlTypeToCSharpType(col.Type, col.IsNullable);
                    string propertyName = NameEditor.PascalCase(col.Name);
                    if (col.IsNullable && !csType.EndsWith("?") && csType != "string")
                        csType += "?";

                    sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}");
                }

                sb.AppendLine("    }");

                // Procedure parameters class
                var parameters = _getSql.GetProcedureParameters(prosedur);
                if (parameters.Any())
                {
                    var paramClassName = $"{className}Params";
                    sb.AppendLine($"    public class {paramClassName}");
                    sb.AppendLine("    {");

                    foreach (var param in parameters)
                    {
                        string csType = TypeMapper.SqlTypeToCSharpType(param.Type, param.IsNullable);
                        string propertyName = NameEditor.PascalCase(param.Name.TrimStart('@'));
                        if (param.IsNullable && !csType.EndsWith("?") && csType != "string") csType += "?";

                        sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}");
                    }

                    sb.AppendLine("    }");
                }

                sb.AppendLine("}");

                SaveFile(path, sb.ToString(), $"{className}.cs");
            }
        }
        catch (Exception ex)
        {
            _getSql.ErrorLog($"Procedure Model Creation Error: {ex.Message}");
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
            sb.AppendLine("        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }");
            sb.AppendLine();

            // DbSets
            foreach (var table in _getSql.GetTables())
            {
                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(table));
                string propName = EnglishInflector.ToPlural(typeName);
                sb.AppendLine($"        public DbSet<{typeName}> {propName} {{ get; set; }}");
            }

            sb.AppendLine();
            sb.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");

            // Unique columns
            foreach (var table in _getSql.GetTables())
            {
                foreach (var unique in _getSql.GetUniqueColmun(table))
                {
                    string entityName = EnglishInflector.ToSingular(NameEditor.PascalCase(unique.TableName));
                    string colName = NameEditor.PascalCase(unique.ColmunName);
                    sb.AppendLine($"            modelBuilder.Entity<{entityName}>().HasIndex(e => e.{colName}).IsUnique();");
                }
            }

            // Foreign keys
            foreach (var fk in _getSql.GetForeignKeys())
            {
                string fkEntity = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.FKTable));
                string pkEntity = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.PKTable));

                string fkPropName = NameEditor.PascalCase(fk.FKColumn);
                if (fkPropName.EndsWith("Id")) fkPropName = fkPropName[..^2];

                string navCollection = EnglishInflector.ToPlural(fkEntity);
                string deleteBehavior = fk.IsNullable ? "DeleteBehavior.SetNull" : "DeleteBehavior.Cascade";

                sb.AppendLine($"            modelBuilder.Entity<{fkEntity}>()");
                sb.AppendLine($"                .HasOne(e => e.{pkEntity})");
                sb.AppendLine($"                .WithMany(e => e.{navCollection})");
                sb.AppendLine($"                .HasForeignKey(e => e.{NameEditor.PascalCase(fk.FKColumn)})");
                sb.AppendLine($"                .OnDelete({deleteBehavior})");
                sb.AppendLine($"                .HasConstraintName(\"FK_{fk.FKTable}_{fk.PKTable}\");");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            SaveFile(path, sb.ToString(), "AppDbContext.cs");
        }
        catch (Exception ex)
        {
            _getSql.ErrorLog($"DbContext Creation Error: {ex.Message}");
        }
    }

    public void GenerateAll(string path, string _namespace)
    {
        tableModelGenerator(path, _namespace);
        ProsedurModelGenerator(path, _namespace);
        DBContextGenerator(path, _namespace);
    }
    public void GenerateAll(string tablePath, string prosedurPath, string dbContextPath, string _namespace)
    {
        tableModelGenerator(tablePath, _namespace);
        ProsedurModelGenerator(prosedurPath, _namespace);
        DBContextGenerator(dbContextPath, _namespace);
    }
    public void GenerateAll(string tablePath, string prosedurPath, string dbContextPath, string table_namespace, string prosedur_namespace, string dbContext_namespace)
    {
        tableModelGenerator(tablePath, table_namespace);
        ProsedurModelGenerator(prosedurPath, prosedur_namespace);
        DBContextGenerator(dbContextPath, dbContext_namespace);
    }

    private void SaveFile(string path, string content, string fileName)
    {
        try
        {
            File.WriteAllText(Path.Combine(path, fileName), content);
            Console.WriteLine($"{fileName} created.");
        }
        catch (Exception ex)
        {
            _getSql.ErrorLog($"File Save Error ({fileName}): {ex.Message}");
        }
    }
}
