using Display.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Display.Providers;

namespace Display.Helper.Data
{
    internal static class DataAccessHelper
    {
        private static string ConnectionString => DataAccess.ConnectionString;

        #region Private

        private static Tuple<bool, SqliteCommand> OpenConnection(string commandText, ref SqliteConnection connection)
        {
            bool isNeedCloseConnection;
            if (connection == null)
            {
                isNeedCloseConnection = true;
                connection = new SqliteConnection(ConnectionString);
            }
            else
            {
                isNeedCloseConnection = false;
            }

            if (connection.State != ConnectionState.Open) connection.Open();

            var command = new SqliteCommand
                (commandText, connection);

            return new Tuple<bool, SqliteCommand>(isNeedCloseConnection, command);
        }

        private static T[] CloseConnectionIfNeedAndReturn<T>(SqliteConnection connection, bool isNeedCloseConnection, List<T> data)
        {
            if (!isNeedCloseConnection) return data?.ToArray();

            connection.Close();
            connection.Dispose();
            return data?.ToArray();
        }

        private static T CloseConnectionIfNeedAndReturn<T>(SqliteConnection connection, bool isNeedCloseConnection, T data)
        {
            if (!isNeedCloseConnection) return data;

            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Close();
            connection.Dispose();
            return data;
        }

        private static async Task<T> CloseConnectionIfNeedAndReturnAsync<T>(SqliteConnection connection, bool isNeedCloseConnection, T data)
        {
            if (!isNeedCloseConnection) return data;

            await connection.CloseAsync();
            await connection.DisposeAsync();
            return data;
        }

        private static void CloseConnectionIfNeed(SqliteConnection connection, bool isNeedCloseConnection)
        {
            if (!isNeedCloseConnection) return;

            connection.Close();
            connection.Dispose();
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 执行命令但不查询
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connection"></param>
        public static void ExecuteNonQuery(string commandText, SqliteConnection connection)
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            command.ExecuteNonQuery();

            CloseConnectionIfNeed(connection, isNeedCloseConnection);
        }

        /// <summary>
        /// 执行命令（带参数）但不查询
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameterCollection"></param>
        /// <param name="connection"></param>
        public static void ExecuteNonQueryWithParameters(string commandText, IEnumerable<SqliteParameter> parameterCollection, SqliteConnection connection)
        {

            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            command.Parameters.AddRange(parameterCollection);
            command.ExecuteNonQuery();

            CloseConnectionIfNeed(connection, isNeedCloseConnection);
        }

        /// <summary>
        /// 异步执行命令（带参数）但不查询
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameterCollection"></param>
        /// <param name="connection"></param>
        public static async Task ExecuteNonQueryWithParametersAsync(string commandText, IEnumerable<SqliteParameter> parameterCollection, SqliteConnection connection)
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            command.Parameters.AddRange(parameterCollection);

            await command.ExecuteReaderAsync();

            CloseConnectionIfNeed(connection, isNeedCloseConnection);
        }

        #endregion

        #region ExecuteReader

        public static T[] ExecuteReaderGetArray<T>(string commandText, SqliteConnection connection) where T : new()
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            var reader = command.ExecuteReader();

            List<T> data = null;

            while (reader.Read())
            {
                data ??= new List<T>();
                data.Add(reader.Export<T>());
            }

            return CloseConnectionIfNeedAndReturn(connection, isNeedCloseConnection, data);
        }

        public static T ExecuteReaderGetSingle<T>(string commandText, SqliteConnection connection) where T : new()
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            var reader = command.ExecuteReader();

            var data = reader.Read() ? reader.Export<T>() : default;

            return CloseConnectionIfNeedAndReturn(connection, isNeedCloseConnection, data);
        }

        public static async Task<T[]> ExecuteReaderGetArrayAsync<T>(string commandText, SqliteConnection connection) where T : new()
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            var reader = await command.ExecuteReaderAsync();

            List<T> data = null;

            while (reader.Read())
            {
                data ??= [];
                data.Add(reader.Export<T>());
            }

            return CloseConnectionIfNeedAndReturn(connection, isNeedCloseConnection, data);
        }

        #endregion


        #region ExecuteScalar

        public static async Task<T> ExecuteScalarAsync<T>(string commandText, SqliteConnection connection)
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);
            var result = await command.ExecuteScalarAsync();

            var data = result.GetNullableAndStructValue<T>();

            return await CloseConnectionIfNeedAndReturnAsync(connection, isNeedCloseConnection, data);
        }

        public static T ExecuteScalar<T>(string commandText, SqliteConnection connection)
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            var result = command.ExecuteScalar();

            var data = result.GetNullableAndStructValue<T>();

            return CloseConnectionIfNeedAndReturn(connection, isNeedCloseConnection, data);
        }

        public static T ExecuteScalarWithParameters<T>(string commandText, IEnumerable<SqliteParameter> parameterCollection, SqliteConnection connection)
        {
            var (isNeedCloseConnection, command) = OpenConnection(commandText, ref connection);

            command.Parameters.AddRange(parameterCollection);

            var result = command.ExecuteScalar();

            var data = result.GetNullableAndStructValue<T>();

            return CloseConnectionIfNeedAndReturn(connection, isNeedCloseConnection, data);
        }

        #endregion

    }
}
