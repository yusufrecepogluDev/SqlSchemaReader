using DbModelGenerator.Models;
using Microsoft.Data.SqlClient;

namespace DbModelGenerator.Utils
{
    internal class GetSql
    {
        private readonly string _server;
        private readonly string _database;

        public GetSql(string server, string database)
        {
            _server = server;
            _database = database;
        }

        // Tek bağlantı stringi metodu
        private string GetConnectionString()
        {
            return $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        // Bağlantıyı sağlayan merkezi metot
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        // Tabloları getir
        public List<string> GetTable()
        {
            List<string> tabloIsimleri = new List<string>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    tabloIsimleri.Add(reader.GetString(0));
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Tablolar): {ex.Message}");
            }
            return tabloIsimleri;
        }

        // Sütunları getir
        public List<Sutun> GetColumn(string tablo)
        {
            var sutunlar = new List<Sutun>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @Tablo", conn);
                cmd.Parameters.AddWithValue("@Tablo", tablo);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sutunlar.Add(new Sutun
                    {
                        Ad = reader.GetString(0),
                        Tip = reader.GetString(1),
                        Nullable = reader.GetString(2) == "YES"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Sütunlar): {ex.Message}");
            }
            return sutunlar;
        }

        // Prosedürleri getir
        public List<string> GetProcedure()
        {
            var prosedurler = new List<string>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    prosedurler.Add(reader.GetString(0));
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Prosedürler): {ex.Message}");
            }
            return prosedurler;
        }

        // Prosedür parametrelerini getir
        public List<ProsedurParametre> GetProcedureParameter(string prosedur)
        {
            var parametre = new List<ProsedurParametre>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    @"SELECT  
                        p.name AS ParametreAdi,
                        t.name AS VeriTipi,
                        p.max_length AS Uzunluk,
                        p.is_output AS CikisParametresiMi,
                        p.has_default_value AS VarsayilanDegerVarMi
                    FROM sys.parameters p
                    JOIN sys.types t ON p.user_type_id = t.user_type_id
                    WHERE p.object_id = OBJECT_ID(@Prosedur);", conn);
                cmd.Parameters.AddWithValue("@Prosedur", "dbo." + prosedur);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    parametre.Add(new ProsedurParametre
                    {
                        Ad = reader.GetString(0),
                        Tip = reader.GetString(1),
                        IsOutput = reader.GetBoolean(3),
                        Nullable = !reader.IsDBNull(4) && reader.GetBoolean(4)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Prosedür Parametreler): {ex.Message}");
            }
            return parametre;
        }

        // Prosedür sütunlarını getir
        public List<ProsedurSutun> GetProcedureColumn(string prosedur)
        {
            var sutun = new List<ProsedurSutun>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                string sql = @"
                    SELECT 
                        name, 
                        system_type_name, 
                        is_nullable
                    FROM sys.dm_exec_describe_first_result_set(@sql, NULL, 0);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sql", $"EXEC {prosedur}");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sutun.Add(new ProsedurSutun
                    {
                        Ad = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Tip = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Nullable = !reader.IsDBNull(2) && reader.GetBoolean(2)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Prosedür Sütunlar - {prosedur}): {ex.Message}");
            }
            return sutun;
        }

        // Foreign Keyleri getir
        public List<ForeignKeyInfo> GetForeignKey()
        {
            var foreignKeys = new List<ForeignKeyInfo>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    @"SELECT 
                        cp.name AS FK_Column,
                        cr.name AS PK_Column,
                        tp.name AS FK_Table,
                        tr.name AS PK_Table,
                        cp.is_nullable AS PK_Nullable
                    FROM sys.foreign_keys fk
                    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                    INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
                    INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
                    INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
                    INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
                    ORDER BY tp.name, cp.name;", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    foreignKeys.Add(new ForeignKeyInfo
                    {
                        FKColumn = reader.GetString(0),
                        PKColumn = reader.GetString(1),
                        FKTable = reader.GetString(2),
                        PKTable = reader.GetString(3),
                        IsNullable = reader.GetBoolean(4)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Foreign Keys): {ex.Message}");
            }
            return foreignKeys;
        }
    }
}
