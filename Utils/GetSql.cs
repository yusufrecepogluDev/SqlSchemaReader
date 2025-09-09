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

        // tableları getir
        public List<string> GetTables()
        {
            List<string> tableNames = new List<string>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    tableNames.Add(reader.GetString(0));
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error (Tables): {ex.Message}");
            }
            return tableNames;
        }

        // Sütunları getir
        public List<Column> GetColumns(string table)
        {
            var columns = new List<Column>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table", conn);
                cmd.Parameters.AddWithValue("@table", table);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(new Column
                    {
                        Name = reader.GetString(0),
                        Type = reader.GetString(1),
                        Nullable = reader.GetString(2) == "YES"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error (Columns): {ex.Message}");
            }
            return columns;
        }

        // Prosedürleri getir
        public List<string> GetProcedures()
        {
            var procedures = new List<string>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    procedures.Add(reader.GetString(0));
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error (Procedures): {ex.Message}");
            }
            return procedures;
        }


        public List<Prosedurparameter> GetProcedureParameters(string prosedur)
        {
            var parameter = new List<Prosedurparameter>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    @"SELECT  
                        p.name AS parameterNamei,
                        t.name AS VeriTypei,
                        p.max_length AS Uzunluk,
                        p.is_output AS CikisparametersiMi,
                        p.has_default_value AS VarsayilanDegerVarMi
                    FROM sys.parameters p
                    JOIN sys.types t ON p.user_type_id = t.user_type_id
                    WHERE p.object_id = OBJECT_ID(@Prosedur);", conn);
                cmd.Parameters.AddWithValue("@Prosedur", "dbo." + prosedur);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    parameter.Add(new Prosedurparameter
                    {
                        Name = reader.GetString(0),
                        Type = reader.GetString(1),
                        IsOutput = reader.GetBoolean(3),
                        Nullable = !reader.IsDBNull(4) && reader.GetBoolean(4)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error (Procedure Parameters): {ex.Message}");
            }
            return parameter;
        }


        public List<ProsedurColumn> GetProcedureColumns(string prosedur)
        {
            var Column = new List<ProsedurColumn>();
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
                    Column.Add(new ProsedurColumn
                    {
                        Name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Type = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Nullable = !reader.IsDBNull(2) && reader.GetBoolean(2)
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error (Procedure Columns - {prosedur}): {ex.Message}");
            }
            return Column;
        }

        public List<ForeignKeyInfo> GetForeignKeys()
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
                Console.WriteLine($"SQL Error (Foreign Keys): {ex.Message}");
            }
            return foreignKeys;
        }
    }
}
