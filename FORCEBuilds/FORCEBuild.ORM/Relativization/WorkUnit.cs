using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.SqlClient;

namespace FORCEBuild.Persistence.Relativization
{
    public class WorkUnit
    {
        private readonly SqlConnection _sqlConnection;
        private readonly Dictionary<DataTable, SqlDataAdapter> _updateMapper;
        public WorkUnit(string datapath, string sqlmodel)
        {
            var connectParam = @"Data Source=" + sqlmodel + ";AttachDbFilename=" + datapath + ";Integrated Security=True;User Instance=True";//(LocalDB)\MSSQLLocalDB
            _sqlConnection = new SqlConnection(connectParam);
            _updateMapper = new Dictionary<DataTable, SqlDataAdapter>();
        }

        public DataTable ExuSqlDataTable(string sql, bool persisted = true)
        {
            _sqlConnection.Open();
            var dataTable = new DataTable();  
            var sqlDataAdapter = new SqlDataAdapter(sql, _sqlConnection);
            sqlDataAdapter.Fill(dataTable);
            if (persisted)
            {
                _updateMapper.Add(dataTable, sqlDataAdapter);
            }
            _sqlConnection.Close();
            return dataTable;
        }

        public bool ExuSql(string sql)
        {
            _sqlConnection.Open();
            var sqlCommand = new SqlCommand(sql, _sqlConnection);
            sqlCommand.ExecuteNonQuery();
            _sqlConnection.Close();
            return true;
        }

        public void Update(DataTable dataTable)
        {
            if (_updateMapper.ContainsKey(dataTable))
            {
                var sqlCommandBuilder = new SqlCommandBuilder(_updateMapper[dataTable]);
                _updateMapper[dataTable].Update(dataTable);
            }
        }

        public void Save(DataTable dataTable,string tablename,Dictionary<string,string> mapping)
        {
            using (var bulkCopy = new SqlBulkCopy(_sqlConnection.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
            {
                bulkCopy.DestinationTableName = tablename;
                foreach (var key in mapping.Keys)
                {

                    bulkCopy.ColumnMappings.Add(key, mapping[key]);
                }
                bulkCopy.WriteToServer(dataTable);
            }
        }

        public void Save(DataTable dataTable, string tablename, List<string> mapping)
        {
            using (var sqlBulk = new SqlBulkCopy(_sqlConnection.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
            {
                sqlBulk.DestinationTableName = tablename;
                foreach (var map in mapping)
                {
                    sqlBulk.ColumnMappings.Add(map, map);
                }
                try
                {
                    sqlBulk.WriteToServer(dataTable);
                    sqlBulk.Close();
                }
                catch(Exception ex)
                {
                    var writer = new StreamWriter(Guid.NewGuid() + ".txt");
                    writer.WriteLine("ERROR:" + ex.Message);
                    foreach(DataRow dataRow in dataTable.Rows)
                    {
                        writer.WriteLine(string.Join(",", dataRow.ItemArray));
                    }
                    writer.Close();
                }
            }
        }
    }
}
