using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;

namespace csh_wpf_ado_pg_northwind_import
{
    public static class QueryHelper
    {

        // for SELECT query 
        public static List<T> ExecuteQuery<T>(string query, Func<NpgsqlDataReader, T> mapFunction, Dictionary<string, object>? parameters = null)
        {
            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow).ShowSqlNotification(query));

            List<T> results = new List<T>();

            using (var cmd = new NpgsqlCommand(query, App.ActiveConnection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(mapFunction(reader)); // mapFunction is a delegate that maps reader to object template
                    }
                }
            }

            return results;

        }

        // for SELECT or CALL query that returns a single value
        public static object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            using (var cmd = new NpgsqlCommand(query, App.ActiveConnection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                return cmd.ExecuteScalar();
            }
        }

        // for INSERT, UPDATE or DELETE
        public static int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow).ShowSqlNotification(query));

            using (var cmd = new NpgsqlCommand(query, App.ActiveConnection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                return cmd.ExecuteNonQuery();
            }
        }

        // for many SQL statements as a single transaction
        public static void ExecuteTransaction(List<string> queries, List<Dictionary<string, object>> parametersList)
        {
            using (var transaction = App.ActiveConnection.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < queries.Count; i++)
                    {
                        using (var cmd = new NpgsqlCommand(queries[i], App.ActiveConnection, transaction)) // keep transaction
                        {
                            if (parametersList[i] != null)
                            {
                                foreach (var param in parametersList[i])
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                                }
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
