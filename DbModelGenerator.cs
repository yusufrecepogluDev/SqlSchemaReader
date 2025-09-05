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

    public void TabloModelGenerator(string klasorYolu, string _namespace)
    {
        try
        {
            foreach (var tablo in _getSql.GetTables())
            {
                List<Sutun> sutunlar = _getSql.GetColumns(tablo);
                var sb = new StringBuilder();
                var className = NameEditor.PascalCase(tablo);
                var dbSetName = className.Length > 1 ? className[..^1] : className;

                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(tablo));
                sb.AppendLine($"namespace {_namespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public class {typeName}");
                sb.AppendLine("    {");

                foreach (var sutun in sutunlar)
                {
                    string csTip = TypeMapper.SqlTipiToCSharpTip(sutun.Tip, sutun.Nullable);
                    string propertyName = NameEditor.PascalCase(sutun.Ad);
                    if (sutun.Nullable && !csTip.EndsWith("?"))
                    {
                        csTip += "?";
                    }
                    sb.AppendLine($"        public {csTip} {propertyName} {{ get; set; }}\n");
                }
                foreach (var fk in _getSql.GetForeignKeys().Where(fk => fk.FKTable == tablo || fk.PKTable == tablo))
                {
                    bool isCollection = fk.PKTable == tablo; // tabloya 1’e-çok FK varsa
                    string relatedType = EnglishInflector.ToSingular(NameEditor.PascalCase(fk.PKTable == tablo ? fk.FKTable : fk.PKTable));
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
                string dosyaAdi = Path.Combine(klasorYolu, $"{NameEditor.PascalCase(tablo)}.cs");
                File.WriteAllText(dosyaAdi, sb.ToString());
                Console.WriteLine($"{dosyaAdi} oluşturuldu.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tablo Model Oluşturma Hatası: {ex.Message}");
        }
    }

    public void ProsedurModelGenerator(string klasorYolu, string _namespace)
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
                            string csTip = TypeMapper.SqlTipiToCSharpTip(col.Tip, col.Nullable);
                            string propertyName = NameEditor.PascalCase(col.Ad);
                            if (col.Nullable && !csTip.EndsWith("?"))
                            {
                                csTip += "?";
                            }
                            sb.AppendLine($"        public {csTip} {propertyName} {{ get; set; }}\n");
                        }

                        sb.AppendLine("    }");
                        sb.AppendLine("}");

                        string dosyaAdi = Path.Combine(klasorYolu, $"{ClassName}.cs");
                        File.WriteAllText(dosyaAdi, sb.ToString());
                        Console.WriteLine($"{dosyaAdi} oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Prosedür Sütun Model Hatası ({prosedur}): {ex.Message}");
                    }
                }
                // Parametre modeli
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
                            string csTip = TypeMapper.SqlTipiToCSharpTip(param.Tip, param.Nullable);
                            string propertyName = NameEditor.PascalCase(param.Ad.TrimStart('@'));
                            if (param.Nullable && !csTip.EndsWith("?"))
                            {
                                csTip += "?";
                            }
                            sb.AppendLine($"        public {csTip} {propertyName} {{ get; set; }}\n");
                        }

                        sb.AppendLine("    }");
                        sb.AppendLine("}");

                        string dosyaAdi = Path.Combine(klasorYolu, $"{ClassName}Params.cs");
                        File.WriteAllText(dosyaAdi, sb.ToString());
                        Console.WriteLine($"{dosyaAdi} oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Prosedür Parametre Model Hatası ({prosedur}): {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Prosedür Model Oluşturma Hatası: {ex.Message}");
        }
    }

    public void DBContextGenerator(string klasorYolu, string _namespace)
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

            foreach (var tablo in _getSql.GetTables())
            {
                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(tablo));
                string propName = EnglishInflector.ToPlural(typeName);// PascalCase ile baş harfleri büyük
                sb.AppendLine($"        public DbSet<{typeName}> {propName} {{ get; set; }}\n");
            }
            sb.AppendLine("    ");
            sb.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");
            foreach (var tablo in _getSql.GetTables())
            {
                sb.AppendLine($"            modelBuilder.Entity<{EnglishInflector.ToSingular(NameEditor.PascalCase(tablo))}>().ToTable(\"{tablo}\");\n");
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
            string dosyaAdi = Path.Combine(klasorYolu, "AppDbContext.cs");
            File.WriteAllText(dosyaAdi, sb.ToString());
            Console.WriteLine($"{dosyaAdi} oluşturuldu.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DbContext Oluşturma Hatası: {ex.Message}");
        }
    }
}
