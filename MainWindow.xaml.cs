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

    private bool IsDebugMode => DebugModeCheckBox.IsChecked == true;


    // ctor
    public MainWindow()
    {
        InitializeComponent();

        // DEBUG
        //App.TestDatabaseConnection();

        LoadConnectionSettings();

        App.ConnectionStatusChanged += status => ConnectionStatus.Text = status;
        App.UpdateConnectionStatus();

        ConnectToDefault();

        LoadCategories();
        LoadProducts();

        LoadCustomersFilter();
        LoadOrders(); 

        LoadCustomers();
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
                DebugLog($"Connections loaded");
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
            string conn = defaultConnectionItem.ConnectionName.ToString();
            //MessageBox.Show($"Connecting to {conn}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Connecting to {conn}");

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
            //MessageBox.Show($"Connecting to {conn}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Connecting to {conn}");

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

    // DEBUG helper
    private void DebugLog(string message)
    {
        if (DebugModeCheckBox.IsChecked == true)
        {
            MessageBox.Show(message, "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // TBD DB workflow

    // TBD adjust width based on content ColumnWidth="*" 
    private void AutoAdjustColumnWidths(DataGrid dataGrid)
    {
        if (dataGrid != null)
        {
            foreach (var column in dataGrid.Columns)
            {
                column.Width = DataGridLength.Auto;
                //column.Width = DataGridLength.SizeToCells; 
            }
        }
    }

    private void LoadCategories()
    {
        string query = "SELECT category_id, category_name FROM categories;";

        var categories = QueryHelper.ExecuteQuery(query, reader => new
        {
            CategoryId = reader.GetInt32(0),
            CategoryName = reader.GetString(1)
        });

        CategoryFilter.ItemsSource = categories;
        CategoryFilter.DisplayMemberPath = "CategoryName";
        CategoryFilter.SelectedValuePath = "CategoryId";

        // default value
        CategoryFilter.SelectedIndex = -1;
    }

    private void LoadProducts(int? categoryId = null)
    {
        string query = "SELECT product_id, product_name, unit_price, units_in_stock, category_id, supplier_id FROM products";

        var parameters = new Dictionary<string, object>();

        if (categoryId.HasValue)
        {
            query += " WHERE category_id = @category";

            parameters.Add("@category", categoryId.Value);
        }

        var products = QueryHelper.ExecuteQuery(query, reader => new
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            UnitPrice = reader.GetFloat(2), // ISSUE check type real to float
            UnitsInStock = reader.GetInt16(3),
            CategoryId = reader.GetInt32(4),
            SupplierId = reader.GetInt32(5)
        }, 
        parameters);

        ProductsGrid.ItemsSource = products;

        //AutoAdjustColumnWidths(ProductsGrid);
    }

    private void LoadCustomersFilter()
    {
        string query = "SELECT DISTINCT customer_id FROM orders WHERE customer_id IS NOT NULL";

        var customers = QueryHelper.ExecuteQuery(query, reader => reader.GetString(0));

        customers.Insert(0, "All Customers"); // item all customers for filtering 

        CustomerFilter.ItemsSource = customers;
        CustomerFilter.SelectedIndex = 0;
    }

    private void LoadOrders(string? customerId = null, bool lastMonthOnly = false)
    {
        string query = "SELECT order_id, customer_id, order_date, ship_country FROM orders";

        var parameters = new Dictionary<string, object>();

        List<string> conditions = new List<string>();

        if (!string.IsNullOrEmpty(customerId))
        {
            conditions.Add("customer_id = @customerId");
            parameters["@customerId"] = customerId;
        }

        if (lastMonthOnly)
        {
            conditions.Add("order_date >= NOW() - INTERVAL '1 month'");
        }

        if (conditions.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }

        var orders = QueryHelper.ExecuteQuery(query, reader => new
        {
            OrderId = reader.GetInt32(0),
            CustomerId = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
            OrderDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
            ShipCountry = reader.IsDBNull(3) ? "N/A" : reader.GetString(3)
        }, 
        parameters);

        OrdersGrid.ItemsSource = orders;
    }

    private void LoadCustomers(string? country = null)
    {
        string query = "SELECT customer_id, company_name, contact_name, country FROM customers";
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(country))
        {
            query += " WHERE country ILIKE @country";
            parameters["@country"] = $"%{country}%";
        }

        var customers = QueryHelper.ExecuteQuery(query, reader => new
        {
            CustomerId = reader.GetString(0),
            CompanyName = reader.GetString(1),
            ContactName = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
            Country = reader.IsDBNull(3) ? "N/A" : reader.GetString(3)
        }, parameters);

        CustomersGrid.ItemsSource = customers;
    }

    public async void ShowSqlNotification(string sqlQuery)
    {
        SqlPopupText.Text = sqlQuery;

        var mainWindow = Application.Current.MainWindow as MainWindow;
        if (mainWindow == null) return;
        await Task.Delay(500); // pause

        // <Popup> Placement="Relative"
        SqlPopup.HorizontalOffset = mainWindow.ActualWidth - 450;
        SqlPopup.VerticalOffset = mainWindow.ActualHeight - 75;

        SqlPopup.IsOpen = true;
        await Task.Delay(3000); // show 3 sec
        SqlPopup.IsOpen = false;
    }


    // TBD ETL workflow
    private void ProcessCsvFile(string filePath)
    {
        CsvDataProcessor processor = new CsvDataProcessor();

        DataTable csvData;

        try
        {
            csvData = processor.ProcessCsv(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //throw;
            return;
        }

        if (csvData.Rows.Count == 0)
        {
            MessageBox.Show("CSV file is empty or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!ValidateCsvData(csvData))
        {
            MessageBox.Show("CSV file contains invalid values", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // DEBUG
        //MessageBox.Show($"CSV file loaded successfully. Rows: {csvData.Rows.Count}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        DebugLog($"CSV file loaded successfully. Rows: {csvData.Rows.Count}");
        
        // pass data to Import tab DataGrid
        ImportedDataGrid.ItemsSource = csvData.DefaultView;
        MessageBox.Show($"CSV loaded. Rows: {csvData.Rows.Count}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private bool ValidateCsvData(DataTable csvData)
    {
        foreach (DataRow row in csvData.Rows)
        {
            foreach (var item in row.ItemArray)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.ToString()))
                {
                    return false; // empty value
                }
                    
            }
        }
        return true;
    }

    private void ImportCsvToDatabase()
    {
        if (ImportedDataGrid.ItemsSource == null)
        {
            MessageBox.Show("No data to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DataTable dataTable = ((DataView)ImportedDataGrid.ItemsSource).ToTable();

        using (var transaction = App.ActiveConnection.BeginTransaction())
        {
            try
            {
                // read column names to snake_case collection
                var columns = ImportedDataGrid.Columns.Select(col => col.Header.ToString().Replace(" ", "_").ToLower()).ToList();
                if (columns.Count == 0) { return; }

                // create temp table "temp_etl" in current session ISSUE
                // drop table if exists
                string dropTableQuery = "DROP TABLE IF EXISTS temp_etl;";
                QueryHelper.ExecuteNonQuery(dropTableQuery);

                // create persistent table "temp_etl" in current session
                string createTableQuery = $@"
                CREATE TABLE temp_etl (
                    {string.Join(", ", columns.Select(c => $"{c} TEXT"))}
                );";
                QueryHelper.ExecuteNonQuery(createTableQuery);

                // insert data into temp table
                string insertTableQuery = $@"
                INSERT INTO temp_etl ({string.Join(", ", columns)}) 
                VALUES ({string.Join(", ", columns.Select(c => $"@{c}"))});";

                foreach (DataRow row in dataTable.Rows)
                {
                    var parameters = new Dictionary<string, object>();
                    foreach (var column in columns)
                    {
                        parameters[$"@{column}"] = row[column] ?? DBNull.Value;
                    }
                    QueryHelper.ExecuteNonQuery(insertTableQuery, parameters);
                }

                // transform data DBA "transform_temp_etl()"

                transaction.Commit();
                MessageBox.Show("Import completed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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
            string selectedConnectionD = ConnectionsList.SelectedItem.GetType().GetProperty("ConnectionName").GetValue(ConnectionsList.SelectedItem).ToString();
            //MessageBox.Show($"Connecting to {selectedConnectionD}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Connecting to {selectedConnectionD}");

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
            // DEBUG
            string selectedCategoryD = CategoryFilter.SelectedItem.ToString();
            //MessageBox.Show($"Filtering by {selectedCategoryD}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Filtering by {selectedCategoryD}");

            // load data from database
            // SQL query: SELECT * FROM Products WHERE Category = selectedCategory
            int selectedCategory = (int)CategoryFilter.SelectedValue;
            LoadProducts(selectedCategory);
        }
        else
        {
            LoadProducts();
        }
    }

    private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryFilter.SelectedItem != null)
        {
            // DEBUG
            string selectedCategory = CategoryFilter.SelectedItem.ToString();
            //MessageBox.Show($"Filtering by {selectedCategory}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Filtering by {selectedCategory}");

            // load data from database
            // SQL query: SELECT * FROM Products WHERE Category = selectedCategory
        }
    }

    private void CustomerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerFilter.SelectedItem != null)
        {
            string selectedCustomer = CustomerFilter.SelectedItem.ToString();
            // DEBUG
            //MessageBox.Show($"Filtering by {selectedCustomer}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Filtering by {selectedCustomer}");

            // load data from database
            // SQL query: SELECT * FROM Orders WHERE Customer = selectedCustomer
            if (selectedCustomer == "All Customers")
            {
                LoadOrders(lastMonthOnly: LastMonthFilter.IsChecked == true);
            }
            else
            {
                LoadOrders(selectedCustomer, lastMonthOnly: LastMonthFilter.IsChecked == true);
            }
        }
    }

    private void LastMonthFilter_Checked(object sender, RoutedEventArgs e)
    {
        // DEBUG
        //MessageBox.Show("Filtering by last month", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        DebugLog($"Filtering by last month");

        // load data from database
        // SQL query: SELECT * FROM Orders WHERE OrderDate >= DATEADD(month, -1, GETDATE())
        LoadOrders(
            customerId: CustomerFilter.SelectedItem?.ToString() == "All Customers" ? null : CustomerFilter.SelectedItem?.ToString(),
            lastMonthOnly: LastMonthFilter.IsChecked == true
            );
    }

    private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrdersGrid.SelectedItem != null)
        {
            // DEBUG
            var selectedOrder = OrdersGrid.SelectedItem;
            //MessageBox.Show($"Selected order: {selectedOrder}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Selected order: {selectedOrder}");
        }
    }

    private void CountryFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        // add text field apply action before executing query

        // DEBUG
        //MessageBox.Show($"Filtering by {inputStr}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // DEBUG
        //MessageBox.Show($"SelectedItem Type: {CustomersGrid.SelectedItem.GetType().FullName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        if (CustomersGrid.SelectedItem != null)
        {
            var selectedCustomer = CustomersGrid.SelectedItem;
            //MessageBox.Show($"Selected customer: {selectedCustomer}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DebugLog($"Selected customer: {selectedCustomer}");
        }

        // TBD open customer details

        if (CustomersGrid.SelectedItem != null)
        {
            // ISSUE no data binding
            CustomerPopup.DataContext = CustomersGrid.SelectedItem;
            CustomerPopup.IsOpen = true;
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
            ProcessCsvFile(filePath);
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
                        //MessageBox.Show("Connection deleted", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        DebugLog("Connection deleted");
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

    private void Button_Click_RefreshProducts(object sender, RoutedEventArgs e)
    {
        CategoryFilter.SelectedValue = null;
        LoadProducts(); 
    }

    private void Button_Click_RefreshOrders(object sender, RoutedEventArgs e)
    {
        CustomerFilter.SelectedItem = "All Customers";
        LastMonthFilter.ClearValue(CheckBox.IsCheckedProperty);
        LoadOrders(); 
    }

    private void ButtonApplyFilter_Click_Apply(object sender, RoutedEventArgs e)
    {
        string inputStr = CountryFilter.Text.Trim();

        // load data from database
        // SQL query: SELECT * FROM Customers WHERE Country LIKE '%{inputStr}%'
        if (string.IsNullOrWhiteSpace(inputStr))
        {
            LoadCustomers();
        }
        else
        {
            LoadCustomers(inputStr);
        }
    }

    private void Button_Click_RefreshCustomers(object sender, RoutedEventArgs e)
    {
        CountryFilter.Text = "";
        LoadCustomers(); // TEMP ignore filter box DONE
    }

    private void Button_Click_NewOrder(object sender, RoutedEventArgs e)
    {
        NewOrderWindow newOrderWindow = new NewOrderWindow();
        newOrderWindow.ShowDialog();
    }

    private void Button_Click_ImportToDb(object sender, RoutedEventArgs e)
    {
        ImportCsvToDatabase();
    }

}