using DbModelGenerator.Models;
using DbModelGenerator.Utils;
using Microsoft.Data.SqlClient;
using System.Text;

namespace DbModelGenerator;
public class DbModelGenerator
{
    private GetSql _getSql;

    public void PostSqlConfig(string server, string database)
    {
        _getSql = new GetSql(server, database);
    }


    public DbModelGenerator(string server, string database)
    {
        _server = server;
        _database = database;
    }

    public void TabloModelGenerator(string klasorYolu, string _namespace)
    {
        try
        {
            foreach (var tablo in GetTable())
            {
                List<Sutun> sutunlar = GetColumns(tablo);
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
                foreach (var fk in GetForeignKey().Where(fk => fk.FKTable == tablo || fk.PKTable == tablo))
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
            foreach (var prosedur in GetProcedure())
            {
                var resultColumns = GetProcedureColumn(prosedur);
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
                var parameters = GetProcedureParameter(prosedur);
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

    public void DBContextGenerator(string klasorYolu)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_database}");
            sb.AppendLine("{");
            sb.AppendLine("    public class AppDbContext : DbContext");
            sb.AppendLine("    {");
            sb.AppendLine("        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }\n");

            foreach (var tablo in GetTable())
            {
                string typeName = EnglishInflector.ToSingular(NameEditor.PascalCase(tablo));
                string propName = EnglishInflector.ToPlural(typeName);// PascalCase ile baş harfleri büyük
                sb.AppendLine($"        public DbSet<{typeName}> {propName} {{ get; set; }}\n");
            }
            sb.AppendLine("    ");
            sb.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");
            foreach (var tablo in GetTable())
            {
                sb.AppendLine($"            modelBuilder.Entity<{EnglishInflector.ToSingular(NameEditor.PascalCase(tablo))}>().ToTable(\"{tablo}\");\n");
            }
            sb.AppendLine("        ");
            sb.AppendLine("        ");
            foreach (var fk in GetForeignKey())
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

    //private List<string> GetTable()
    //{
    //    List<string> tabloIsimleri = new List<string>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        using SqlCommand cmd = new SqlCommand(
    //            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn);

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            tabloIsimleri.Add(reader.GetString(0));
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Tablolar): {ex.Message}");
    //    }
    //    return tabloIsimleri;
    //}

    //private List<Sutun> GetColumns(string tablo)
    //{
    //    List<Sutun> sutunlar = new List<Sutun>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        using SqlCommand cmd = new SqlCommand(
    //            "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @Tablo", conn);
    //        cmd.Parameters.AddWithValue("@Tablo", tablo);

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            sutunlar.Add(new Sutun
    //            {
    //                Ad = reader.GetString(0),
    //                Tip = reader.GetString(1),
    //                Nullable = reader.GetString(2) == "YES"
    //            });
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Sütunlar): {ex.Message}");
    //    }
    //    return sutunlar;
    //}

    //private List<string> GetProcedure()
    //{
    //    List<string> prosedurler = new List<string>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        using SqlCommand cmd = new SqlCommand(
    //            "SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'", conn);

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            prosedurler.Add(reader.GetString(0));
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Prosedürler): {ex.Message}");
    //    }
    //    return prosedurler;
    //}

    //private List<ProsedurParametre> GetProcedureParameter(string prosedur)
    //{
    //    List<ProsedurParametre> parametre = new List<ProsedurParametre>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        using SqlCommand cmd = new SqlCommand(
    //            @"SELECT  
    //                p.name AS ParametreAdi,
    //                t.name AS VeriTipi,
    //                p.max_length AS Uzunluk,
    //                p.is_output AS CikisParametresiMi,
    //                p.has_default_value AS VarsayilanDegerVarMi,
    //                p.default_value AS VarsayilanDeger,
    //                CASE 
    //                    WHEN p.has_default_value = 1 THEN 'Opsiyonel (default değer var)'
    //                    ELSE 'Zorunlu (default değer yok)'
    //                END AS ParametreDurumu
    //            FROM sys.parameters p
    //            JOIN sys.types t ON p.user_type_id = t.user_type_id
    //            WHERE p.object_id = OBJECT_ID(@Prosedur);", conn);
    //        cmd.Parameters.AddWithValue("@Prosedur", "dbo." + prosedur);

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            parametre.Add(new ProsedurParametre
    //            {
    //                Ad = reader.GetString(0),
    //                Tip = reader.GetString(1),
    //                IsOutput = reader.GetBoolean(3),
    //                Nullable = !reader.IsDBNull(4) && reader.GetBoolean(4)
    //            });
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Prosedür Parametreler): {ex.Message}");
    //    }
    //    return parametre;
    //}

    //private List<ProsedurSutun> GetProcedureColumn(string prosedur)
    //{
    //    List<ProsedurSutun> sutun = new List<ProsedurSutun>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        string sql = @"
    //        SELECT 
    //            name, 
    //            system_type_name, 
    //            is_nullable
    //        FROM sys.dm_exec_describe_first_result_set(@sql, NULL, 0);";

    //        using SqlCommand cmd = new SqlCommand(sql, conn);
    //        cmd.Parameters.AddWithValue("@sql", $"EXEC {prosedur}");

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            sutun.Add(new ProsedurSutun
    //            {
    //                Ad = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
    //                Tip = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
    //                Nullable = !reader.IsDBNull(2) && reader.GetBoolean(2)
    //            });
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Prosedür Sütunlar - {prosedur}): {ex.Message}");
    //    }
    //    return sutun;
    //}

    //private List<ForeignKeyInfo> GetForeignKey()
    //{
    //    List<ForeignKeyInfo> foreignKeys = new List<ForeignKeyInfo>();
    //    string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
    //    try
    //    {
    //        using SqlConnection conn = new SqlConnection(connStr);
    //        conn.Open();

    //        using SqlCommand cmd = new SqlCommand(
    //            @"SELECT 
    //                cp.name AS FK_Column,
    //                cr.name AS PK_Column,
    //                tp.name AS FK_Table,
    //                tr.name AS PK_Table,
    //                cp.is_nullable AS PK_Nullable
    //            FROM sys.foreign_keys fk
    //            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    //            INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
    //            INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
    //            INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
    //            INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
    //            ORDER BY tp.name, cp.name;", conn);

    //        using SqlDataReader reader = cmd.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            foreignKeys.Add(new ForeignKeyInfo
    //            {
    //                FKColumn = reader.GetString(0),
    //                PKColumn = reader.GetString(1),
    //                FKTable = reader.GetString(2),
    //                PKTable = reader.GetString(3),
    //                IsNullable = reader.GetBoolean(4)
    //            });
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        Console.WriteLine($"SQL Hatası (Foreign Keys): {ex.Message}");
    //    }
    //    return foreignKeys;
    //}
}
