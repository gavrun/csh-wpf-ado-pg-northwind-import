using Npgsql;
using System.Configuration;
using System.Data;
using System.Windows;

namespace csh_wpf_ado_pg_northwind_import;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // only one active connection at a time
    // allow not having active connection
    public static NpgsqlConnection? ActiveConnection { get; private set; }


    // DEBUG initial test
    public static void TestDatabaseConnection()
    {
        //string connString = "Server=localhost;Port=5432;User Id=sa;Password=sa;Database=northwind;"; 

        string connString = ConfigurationManager.ConnectionStrings["TestPostgreSqlConnection"].ConnectionString;

        try
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                MessageBox.Show("Connection successful", "Connection test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Connection test", MessageBoxButton.OK, MessageBoxImage.Error);
            //throw;
        }
    }

    public static void SetActiveConnection(NpgsqlConnection connection)
    {
        // close active connections explicitly

        if (ActiveConnection != null && (ActiveConnection.State == ConnectionState.Open || ActiveConnection.State == ConnectionState.Connecting))
        {
            ActiveConnection.Close();
            ActiveConnection.Dispose();
        }

        NpgsqlConnection.ClearAllPools();

        ActiveConnection = connection;
    }

    // TBD test periodically if active connection is available

    //public static void TestActiveWatcher()
    //{
    //    if ( POOL active CONNECTION idle )
    //    {
    //        throw new InvalidOperationException("Connection lost");
    //    }
    //}

}

