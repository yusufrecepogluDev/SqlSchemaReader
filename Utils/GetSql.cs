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

        
        private string GetConnectionString()
        {
            return $"Server={_server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        
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
                string message = $"SQL Error (Tables): {ex.Message}";
                ErrorLog(message);
            }
            return tableNames;
        }

        
        public List<Column> GetColumns(string table)
        {
            var columns = new List<Column>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME, DATA_TYPE ,CHARACTER_MAXIMUM_LENGTH, IS_Nullable FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table", conn);
                cmd.Parameters.AddWithValue("@table", table);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(new Column
                    {
                        Name = reader.GetString(0),
                        Type = reader.GetString(1),
                        TypeLength = reader.IsDBNull(2) ? -2 : reader.GetInt32(2),
                        IsNullable = reader.GetString(3) == "YES"
                    });
                }
            }
            catch (SqlException ex)
            {
                string message = $"SQL Error (Columns): {ex.Message}";
                ErrorLog(message);
            }
            return columns;
        }

        
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
                string message = $"SQL Error (Procedures): {ex.Message}";
                ErrorLog(message);
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
                        IsNullable = !reader.IsDBNull(4) && reader.GetBoolean(4)
                    });
                }
            }
            catch (SqlException ex)
            {
                string message = $"SQL Error (Procedure Parameters): {ex.Message}";
                ErrorLog(message);
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
                        is_Nullable
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
                        IsNullable = !reader.IsDBNull(2) && reader.GetBoolean(2)
                    });
                }
            }
            catch (SqlException ex)
            {
                string message = $"SQL Error (Procedure Columns - {prosedur}): {ex.Message}";
                ErrorLog(message);
            }
            return Column;
        }

        public List<UniqueColmun> GetUniqueColmun(string _table)
        {
            var uniqueColmuns = new List<UniqueColmun>();
            try
            {
                using var conn = CreateConnection();
                conn.Open();
                using var cmd = new SqlCommand(
                    @"SELECT 
                        col.name AS ColumnName,
                        tab.name AS TableName
                    FROM sys.indexes idx
                    INNER JOIN sys.index_columns ic ON idx.object_id = ic.object_id AND idx.index_id = ic.index_id
                    INNER JOIN sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id
                    INNER JOIN sys.tables tab ON idx.object_id = tab.object_id
                    WHERE idx.is_unique = 1 AND idx.is_primary_key = 0 AND tab.name = @tablo
                    ORDER BY tab.name, col.name", conn);
                cmd.Parameters.AddWithValue("@tablo", _table);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    uniqueColmuns.Add(new UniqueColmun
                    {
                        ColmunName = reader.GetString(0),
                        TableName = reader.GetString(1)
                    });
                }
            }
            catch (SqlException ex)
            {
                string message = $"SQL Error (Unique Columns): {ex.Message}";
                ErrorLog(message);
            }
            return uniqueColmuns;
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
                        cp.is_Nullable AS PK_IsNullable
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
                string message = $"SQL Error (Foreign Keys): {ex.Message}";
                ErrorLog(message);
            }
            return foreignKeys;
        }

        public void ErrorLog(string message)
        {
            Console.WriteLine($"Error: {message}");
        }

    }
}
