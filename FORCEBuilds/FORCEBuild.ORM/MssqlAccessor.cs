#define output

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace FORCEBuild.ORM
{
    internal class MssqlAccessor : Accessor
    {
        private SqlConnection _connection;

        private SqlCommand CreateCommand(DataBaseCommand baseCommand, SqlConnection connection)
        {
            var command = connection.CreateCommand();
            var type = baseCommand.GetType().Name;
            string sqlSentence;
            switch (type)
            {
                case "InsertCommand":
                    var insertCommand = (InsertCommand)baseCommand;
                    if (insertCommand.InsertPairs.Count==0)
                    {
                        sqlSentence = "INSERT INTO " + insertCommand.TableName;
                        command.CommandText = sqlSentence;
                    }
                    else
                    {
                        sqlSentence = "INSERT INTO " + insertCommand.TableName + " (";
                        sqlSentence = insertCommand.InsertPairs
                            .Aggregate(sqlSentence, (current, column) => current + column.Column + ",");
                        sqlSentence = sqlSentence.TrimEnd(',');
                        sqlSentence += ") VALUES (";
                        sqlSentence = insertCommand.InsertPairs
                            .Aggregate(sqlSentence, (current, column) => current + "@" + column.Column + ",");
                        sqlSentence = sqlSentence.TrimEnd(',');
                        sqlSentence += ")";
                        command.CommandText = sqlSentence;
                        foreach (var t in insertCommand.InsertPairs)
                            command.Parameters.AddWithValue("@" + t.Column, t.Value);
                    }
                    break;
                case "UpdateCommand":
                    var updateCommand = (UpdateCommand)baseCommand;
                    sqlSentence = "UPDATE " + updateCommand.TableName + " SET ";
                    sqlSentence = updateCommand.UpdatePairs
                        .Aggregate(sqlSentence, (current, updatePair) => current + updatePair.Column + " = @" + updatePair.Column + ",");
                    sqlSentence = sqlSentence.TrimEnd(',');
                    sqlSentence += " WHERE ";
                    //暂时只需要针对有主键表的更新
                    var len = updateCommand.ConditionPairs.Count - 1;
                    for (var index = 0; index < len; index++)
                    {
                        var conditionPair = updateCommand.ConditionPairs[index];
                        sqlSentence += conditionPair.Column + " = @2" + conditionPair.Column + " AND ";
                    }
                    sqlSentence += updateCommand.ConditionPairs[len].Column + " = @2" + updateCommand.ConditionPairs[len].Column;
                    command.CommandText = sqlSentence;
                    foreach (var ucConditionPair in updateCommand.ConditionPairs)
                        command.Parameters.AddWithValue("@2" + ucConditionPair.Column, ucConditionPair.Value);
                    foreach (var ucUpdatePair in updateCommand.UpdatePairs)
                        command.Parameters.AddWithValue("@" + ucUpdatePair.Column, ucUpdatePair.Value);
                    break;
                case "DeleteCommand":
                    //只删除单个值匹配情况
                    var deleteCommand = (DeleteCommand)baseCommand;
                    sqlSentence = "DELETE FROM " + deleteCommand.TableName;
                    if (deleteCommand.ConditionPairs.Count==0)
                    {
                        command.CommandText = sqlSentence;
                        break;
                    }
                    sqlSentence += " WHERE ";
                    len = deleteCommand.ConditionPairs.Count - 1;
                    for (var i = 0; i < len; ++i)
                    {
                        var pair = deleteCommand.ConditionPairs[i];
                        sqlSentence += pair.Column + " = @" + pair.Column + " AND ";
                    }
                    sqlSentence += deleteCommand.ConditionPairs[len].Column + " = @" + deleteCommand.ConditionPairs[len].Column;
                    command.CommandText = sqlSentence;
                    foreach (var conditionPair in deleteCommand.ConditionPairs)
                        command.Parameters.AddWithValue("@" + conditionPair.Column, conditionPair.Value);
                    break;
                default:
                    var selectCommand = (SelectCommand)baseCommand;
                    sqlSentence = "SELECT * FROM " + baseCommand.TableName;
                    if (selectCommand.ConditionPairs == null)
                    {
                        command.CommandText = sqlSentence;
                        break;
                    };
                    if (selectCommand.ConditionPairs.Count == 0)
                    {
                        command.CommandText = sqlSentence;
                        break;
                    }
                    sqlSentence += " WHERE ";
                    len = selectCommand.ConditionPairs.Count;
                    for (var i = 0; i < len - 1; ++i)
                    {
                        var pair = selectCommand.ConditionPairs[i];
                        sqlSentence += pair.Column + " = @" + pair.Column + " AND ";
                    }
                    sqlSentence += selectCommand.ConditionPairs[len - 1].Column + " = @" + selectCommand.ConditionPairs[len - 1].Column;
                    command.CommandText = sqlSentence;
                    foreach (var conditionPair in selectCommand.ConditionPairs)
                    {
                        //command.Parameters.AddWithValue(, conditionPair.Value);
                        command.Parameters.Add(new SqlParameter("@" + conditionPair.Column, conditionPair.Value));
                    }
                    break;
            }
#if output
            Log.Write(sqlSentence);
#endif
            return command;
        }

        protected override void ExecuteSql(DataBaseCommand baseCommand)
        {
            var connection = GetConnection;
            var command = CreateCommand(baseCommand, connection);
            var sqlTransaction = connection.BeginTransaction(Guid.NewGuid().ToString());
            command.Connection = connection;
            command.Transaction = sqlTransaction;
            try
            {
                command.ExecuteNonQuery();
                sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw new Exception("数据库储存执行失败，已回滚",e);
            }  
        }

        /// <summary>
        /// 返回最后一个自动标识
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        protected override int InsertSql(InsertCommand insert)
        {
            int guid;
            var connection = GetConnection;
            var sqlTransaction = connection.BeginTransaction(Guid.NewGuid().ToString());
            var command = CreateCommand(insert, connection);
            command.CommandText += "; SELECT @@IDENTITY as '" + insert.IdColumn + "'";
            command.Connection = connection;
            command.Transaction = sqlTransaction;
            try
            {
                guid = Convert.ToInt32(command.ExecuteScalar());
                sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw new DataException("未能将记录成功写入数据库",e);
            }
            return guid;
        }

        protected override DataTable Read(SelectCommand selectCommand)
        {
            var table = new DataTable();
            using (var cmd = CreateCommand(selectCommand, GetConnection))
            using (var sda = new SqlDataAdapter(cmd))
                sda.Fill(table);
            return table;
        }

        protected override DataTable Read(string query)
        {
            var table = new DataTable();
            using (var sda = new SqlDataAdapter(query, GetConnection))
                sda.Fill(table);
            return table;
        }

        private SqlConnection GetConnection
        {
            get
            {
                if (_connection == null)
                    _connection = new SqlConnection(ConnectionString);
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                return _connection;
            }
        }

        public override void Close()
        {
            base.Close();
            if (_connection == null) return;
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }
    }
}
