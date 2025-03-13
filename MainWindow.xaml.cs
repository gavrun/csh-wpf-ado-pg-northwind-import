using Microsoft.Win32;
using Npgsql;
using System.Configuration;
using System.Data;
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

    private ConnectionItem defaultConnectionItem;

    private NpgsqlConnection activeConnection;


    // ctor
    public MainWindow()
    {
        InitializeComponent();

        // DEBUG
        //App.TestDatabaseConnection();

        LoadConnectionSettings();

        ConnectToDefault();

        //LoadProducts();
        //LoadOrders(); 
        //LoadCustomers();
    }
    
    // read dbconfig.xml when starting 
    public void LoadConnectionSettings()
    {
        if (File.Exists(dbConfigPath))
        {
            try
            {
                // load connections from xml
                XDocument configXml = XDocument.Load(dbConfigPath);

                XElement root = configXml.Element("ConnectionSettings");

                //List<Dictionary<string, string>> connections = new List<Dictionary<string, string>>();
                List<ConnectionItem> connections = new List<ConnectionItem>();

                ConnectionsList.ItemsSource = null;

                foreach (XElement conn in root.Elements("Connection"))
                {
                    connections.Add(new ConnectionItem
                    {
                        ConnectionName = conn.Element("ConnectionName")?.Value,
                        Provider = conn.Element("Provider")?.Value,
                        IsDefault = bool.TryParse(conn.Element("IsDefault")?.Value, out bool result) && result,
                        Host = conn.Element("Host")?.Value,
                        Port = conn.Element("Port")?.Value,
                        Database = conn.Element("Database")?.Value,
                        User = conn.Element("User")?.Value,
                        Password = conn.Element("Password")?.Value
                    });
                }

                // pass connections to UI
                ConnectionsList.ItemsSource = connections;

                defaultConnectionItem = connections.FirstOrDefault(c => c.IsDefault);

                // DEBUG
                //MessageBox.Show("Connections loaded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private void ConnectToDefault()
    {
        if (defaultConnectionItem != null)
        {
            // DEBUG
            //string conn = defaultConnectionItem.ConnectionName.ToString();
            //MessageBox.Show($"Connecting to {conn}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // connect to default connection
            ConnectToDatabase(defaultConnectionItem);
        }
    }

    private void ConnectToDatabase(ConnectionItem selectedConnection)
    {
        // close active connections explicitly

        // new connection
        string connString = selectedConnection.GetConnectionString();

        var connection = new NpgsqlConnection(connString);

        try
        {
            connection.Open();
            App.SetActiveConnection(connection);

            // DEBUG
            string conn = selectedConnection.ConnectionName.ToString();
            MessageBox.Show($"Connecting to {conn}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // push status to UI
            //ConnectionStatus.Text = $"Status: Connected to {selectedConnection.ConnectionName}";

            // push data to UI
            // TBD
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error connecting to selected connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //throw;
        }
    }

    // TBD ETL workflow
    private void ProcessCsvFile(string filePath)
    {
        CsvDataProcessor processor = new CsvDataProcessor();

        DataTable csvData = processor.ProcessCsv(filePath);

        if (csvData.Rows.Count == 0)
        {
            MessageBox.Show("CSV file is empty or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // TBD pass data to Import tab DataGrid
        //ImportedDataGrid.ItemsSource = csvData.DefaultView;

        MessageBox.Show($"CSV file loaded successfully. Rows: {csvData.Rows.Count}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // event handlers
    private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
    {
        //close database connection 

        //save connection settings if not saved
        //SaveConnectionSettings()

        Application.Current.Shutdown();
    }

    private void MenuItem_Click_ToggleSidePanel(object sender, RoutedEventArgs e)
    {
        SidePanel.Visibility = (SidePanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void MenuItem_Click_NewConnection(object sender, RoutedEventArgs e)
    {
        ConnectionWindow connectionWindow = new ConnectionWindow(false);
        connectionWindow.ShowDialog();
    }

    private void MenuItem_Click_ShowConnectionsPanel(object sender, RoutedEventArgs e)
    {
        // force open ISSUE check mark
        
        if (SidePanel.Visibility == Visibility.Collapsed)
        {
            SidePanel.Visibility = Visibility.Visible;
            MenuItemSidePanel.IsChecked = true;
        }
    }

    private void MenuItem_Click_ToggleImportTab(object sender, RoutedEventArgs e)
    {
        //force open
        ImportTab.Visibility = Visibility.Visible;

        //hide after button upload to database is clicked and successful sql query is executed
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
            // DEBUG
            //string selectedConnection = ConnectionsList.SelectedItem.GetType().GetProperty("ConnectionName").GetValue(ConnectionsList.SelectedItem).ToString();
            //MessageBox.Show($"Connecting to {selectedConnection}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            if (ConnectionsList.SelectedItem is ConnectionItem selectedConnection)
            {
                ConnectToDatabase(selectedConnection);
            }
        }
        else
        {
            // DEBUG
            //MessageBox.Show("Select connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (defaultConnectionItem != null)
            {
                string conn = defaultConnectionItem.ConnectionName.ToString();
                MessageBox.Show($"Connecting to default {conn}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Select connection", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void EditButton_Click_EditConnection(object sender, RoutedEventArgs e)
    {
        if (ConnectionsList.SelectedItem is ConnectionItem selectedConnection)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(true, selectedConnection);
            connectionWindow.ShowDialog();
        }
        else
        {
            MessageBox.Show("Select connection", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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

    private void Button_Click_OpenFile(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog 
        {
            Filter = "CSV Files (*.csv)|*.csv",
            Title = "Select a CSV file"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;

            MessageBox.Show($"Selected file: {filePath}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // TBD read CSV file
            //ProcessCsvFile(filePath);
        }
    }

    private void DeleteButton_Click_DeleteConnection(object sender, RoutedEventArgs e)
    {
        if (ConnectionsList.SelectedItem is ConnectionItem selectedConnection)
        {
            MessageBoxResult result = MessageBox.Show($"Delete connection {selectedConnection.ConnectionName}?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    XDocument configXml = XDocument.Load(dbConfigPath);
                    XElement root = configXml.Element("ConnectionSettings");

                    XElement connectionToDelete = root
                        .Elements("Connection")
                        .FirstOrDefault(c => c.Element("ConnectionName")?.Value == selectedConnection.ConnectionName);

                    if (connectionToDelete != null)
                    {
                        connectionToDelete.Remove();
                        configXml.Save(dbConfigPath);

                        LoadConnectionSettings();

                        // DEBUG
                        MessageBox.Show("Connection deleted", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
        }
        else
        {
            MessageBox.Show("Select connection", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}