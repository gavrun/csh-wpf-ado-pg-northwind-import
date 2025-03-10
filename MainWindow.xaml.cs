using Npgsql;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

                string host = root.Element("Host")?.Value;
                string port = root.Element("Port")?.Value;
                string database = root.Element("Database")?.Value;
                string user = root.Element("User")?.Value;
                string password = root.Element("Password")?.Value;

                string connString = $"Server={host};Port={port};User Id={user};Password={password};Database={database};";

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

    private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
    {
        //close database connection 

        //save connection settings if not saved

        Application.Current.Shutdown();
    }

    private void MenuItem_Click_ToggleSidePanel(object sender, RoutedEventArgs e)
    {
        SidePanel.Visibility = (SidePanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void MenuItem_Click_Connect(object sender, RoutedEventArgs e)
    {
        ConnectionWindow connectionWindow = new ConnectionWindow();
        connectionWindow.ShowDialog();
    }

    private void MenuItem_Click_ShowConnectionsPanel(object sender, RoutedEventArgs e)
    {
        // force open
        SidePanel.Visibility = Visibility.Visible;
    }

    private void MenuItem_Click_ToggleImportTab(object sender, RoutedEventArgs e)
    {
        //ImportTab.Visibility = (ImportTab.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        
        //force open
        ImportTab.Visibility = Visibility.Visible;

        //hide after button upload to database is clicked
    }

    private void MenuItem_Click_About(object sender, RoutedEventArgs e)
    {
        //MessageBox.Show("Northwind Importer\nVersion 1.0\n\n(c) 2021", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        MessageBox.Show("Northwind Traders\n\nVersion x.x\n\n2025\n\n[TBD data provenance note TBD]\n\nNorthwind is a training (demo) database, originally developed by Microsoft as an example of working with relational DBMS. It contains sales data of a fictitious trading company Northwind Traders, which is engaged in the export and import of food products worldwide. This database models typical processes of a small business: product management (assortment and stock), registration of customer orders, work with suppliers, delivery of goods, as well as the work of employees responsible for sales.\n", "About", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ConnectButton_Click_Connect(object sender, RoutedEventArgs e)
    {
        if (ConnectionsList.SelectedItem != null)
        {
            string selectedConnection = ConnectionsList.SelectedItem.ToString();

            MessageBox.Show($"Connecting to {selectedConnection}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Select connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditButton_Click_EditConnection(object sender, RoutedEventArgs e)
    {
        ConnectionWindow connectionWindow = new ConnectionWindow();
        connectionWindow.ShowDialog();
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryFilter.SelectedItem != null)
        {
            string selectedCategory = CategoryFilter.SelectedItem.ToString();

            MessageBox.Show($"Filtering by {selectedCategory}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // load data from database
            // SQL query: SELECT * FROM Products WHERE Category = selectedCategory
        }
    }

    private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryFilter.SelectedItem != null)
        {
            string selectedCategory = CategoryFilter.SelectedItem.ToString();

            MessageBox.Show($"Filtering by {selectedCategory}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // load data from database
            // SQL query: SELECT * FROM Products WHERE Category = selectedCategory
        }
    }

    private void CustomerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerFilter.SelectedItem != null)
        {
            string selectedCustomer = CustomerFilter.SelectedItem.ToString();

            MessageBox.Show($"Filtering by {selectedCustomer}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // load data from database
            // SQL query: SELECT * FROM Orders WHERE Customer = selectedCustomer
        }
    }

    private void LastMonthFilter_Checked(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Filtering by last month", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        // load data from database
        // SQL query: SELECT * FROM Orders WHERE OrderDate >= DATEADD(month, -1, GETDATE())
    }

    private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TBD
        if (OrdersGrid.SelectedItem != null)
        {
            var selectedOrder = OrdersGrid.SelectedItem;

            MessageBox.Show($"Selected order: {selectedOrder}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // TBD open new window with order details
        }
    }

    private void CountryFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        string inputStr = CountryFilter.Text;

        // add text field enter action before executing query

        MessageBox.Show($"Filtering by {inputStr}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
   
        // load data from database
        // SQL query: SELECT * FROM Customers WHERE Country LIKE '%{inputStr}%'
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TBD
        if (CustomersGrid.SelectedItem != null)
        {
            var selectedCustomer = CustomersGrid.SelectedItem;

            MessageBox.Show($"Selected customer: {selectedCustomer}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // TBD open new window with customer details
        }
    }
}