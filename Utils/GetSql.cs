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
        private static string GetConSql(string server, string database)
        {
            return $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;";

        }


        public static List<string> GetTable()
        {
            List<string> tabloIsimleri = new List<string>();
            try
            {
                using SqlConnection conn = new SqlConnection(GetConSql(_server, _database));
                conn.Open();

                using SqlCommand cmd = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tabloIsimleri.Add(reader.GetString(0));
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Tablolar): {ex.Message}");
            }
            return tabloIsimleri;
        }

        public static List<Sutun> GetColumns(string tablo)
        {
            List<Sutun> sutunlar = new List<Sutun>();
            string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using SqlCommand cmd = new SqlCommand(
                    "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @Tablo", conn);
                cmd.Parameters.AddWithValue("@Tablo", tablo);

                using SqlDataReader reader = cmd.ExecuteReader();
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

        public static List<string> GetProcedure()
        {
            List<string> prosedurler = new List<string>();
            string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using SqlCommand cmd = new SqlCommand(
                    "SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'", conn);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    prosedurler.Add(reader.GetString(0));
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Hatası (Prosedürler): {ex.Message}");
            }
            return prosedurler;
        }

        public static List<ProsedurParametre> GetProcedureParameter(string prosedur)
        {
            List<ProsedurParametre> parametre = new List<ProsedurParametre>();
            string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using SqlCommand cmd = new SqlCommand(
                    @"SELECT  
                    p.name AS ParametreAdi,
                    t.name AS VeriTipi,
                    p.max_length AS Uzunluk,
                    p.is_output AS CikisParametresiMi,
                    p.has_default_value AS VarsayilanDegerVarMi,
                    p.default_value AS VarsayilanDeger,
                    CASE 
                        WHEN p.has_default_value = 1 THEN 'Opsiyonel (default değer var)'
                        ELSE 'Zorunlu (default değer yok)'
                    END AS ParametreDurumu
                FROM sys.parameters p
                JOIN sys.types t ON p.user_type_id = t.user_type_id
                WHERE p.object_id = OBJECT_ID(@Prosedur);", conn);
                cmd.Parameters.AddWithValue("@Prosedur", "dbo." + prosedur);

                using SqlDataReader reader = cmd.ExecuteReader();
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

        public static List<ProsedurSutun> GetProcedureColumn(string prosedur)
        {
            List<ProsedurSutun> sutun = new List<ProsedurSutun>();
            string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                string sql = @"
            SELECT 
                name, 
                system_type_name, 
                is_nullable
            FROM sys.dm_exec_describe_first_result_set(@sql, NULL, 0);";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sql", $"EXEC {prosedur}");

                using SqlDataReader reader = cmd.ExecuteReader();
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

        public static List<ForeignKeyInfo> GetForeignKey()
        {
            List<ForeignKeyInfo> foreignKeys = new List<ForeignKeyInfo>();
            string connStr = $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using SqlCommand cmd = new SqlCommand(
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

                using SqlDataReader reader = cmd.ExecuteReader();
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
