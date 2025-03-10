using Npgsql;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace csh_wpf_ado_pg_northwind_import;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //
    private string dbConfigPath = "dbconfig.xml";


    // ctor
    public MainWindow()
    {
        InitializeComponent();

        //TestDatabaseConnection();
        LoadConnectionSettings();
    }

    // DEBUG
    private void TestDatabaseConnection()
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

    // read dbconfig.xml when starting 
    private void LoadConnectionSettings()
    {
        if (File.Exists(dbConfigPath))
        {
            try
            {
                XDocument configXml = XDocument.Load(dbConfigPath);

                XElement root = configXml.Element("ConnectionSettings");

                string server = root.Element("Host")?.Value;
                string port = root.Element("Port")?.Value;
                string database = root.Element("Database")?.Value;
                string user = root.Element("User")?.Value;
                string password = root.Element("Password")?.Value;

                string connString = $"Server={server};Port={port};User Id={user};Password={password};Database={database};";

                MessageBox.Show("Connections loaded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading connections: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw;
            }
        }
        else
        {
            MessageBox.Show("Connections were not saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // save dbconfig.xml when closing
    private void SaveConnectionSettings(string host, string port, string database, string user, string password)
    {
        try
        {
            XDocument configXml = new XDocument(

                new XElement("ConnectionSettings",
                    new XElement("Host", host),
                    new XElement("Port", port),
                    new XElement("Database", database),
                    new XElement("User", user),
                    new XElement("Password", password)
                )
            );
            configXml.Save(dbConfigPath);

            MessageBox.Show("Connections saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {

            throw;
        }
    }



}