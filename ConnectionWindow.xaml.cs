using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace csh_wpf_ado_pg_northwind_import
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        //
        private string dbConfigPath = "dbconfig.xml";

        private bool isEditMode;

        private ConnectionItem selectedConnection;

        // ctor
        public ConnectionWindow(bool isEdit = false, ConnectionItem connection = null)
        {
            InitializeComponent();
            isEditMode = isEdit;

            selectedConnection = connection;

            if (isEditMode && selectedConnection != null)
            {
                // default before load existing
                // XAML ISSUE Text="localhost" > FallbackValue="localhost" > HostTextBox.Text = "localhost"

                LoadExistingSettings();
            }
        }

        //
        private void LoadExistingSettings()
        {
            try
            {
                ConnectionNameTextBox.Text = selectedConnection.ConnectionName;

                if (selectedConnection.Provider == "Npgsql")
                {
                    PostgreSQLRadioButton.IsChecked = true; //default
                }
                //else if (selectedConnection.Provider == "SqlClient")
                //{
                //    MSSQLRadioButton.IsChecked = true;
                //}

                DefaultConnectionCheckBox.IsChecked = selectedConnection.IsDefault;

                HostTextBox.Text = selectedConnection.Host;
                PortTextBox.Text = selectedConnection.Port;
                DatabaseTextBox.Text = selectedConnection.Database;
                UserTextBox.Text = selectedConnection.User;

                PasswordBox.Password = selectedConnection.Password;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool Validate(TextBox textBox)
        {
            //textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource(); ISSUE with UpdateSource()

            BindingExpression binding = textBox.GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
            {
                binding.UpdateSource();
            }

            return !Validation.GetHasError(textBox);
        }

        private void TestConnectionButton_Click_TestConnection(object sender, RoutedEventArgs e)
        {
            //
            MessageBox.Show("Testing connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // check connection state Npgsql

            string connString = $"Host={HostTextBox.Text.Trim()};" +
                                $"Port={PortTextBox.Text.Trim()};" +
                                $"Database={DatabaseTextBox.Text.Trim()};" +
                                $"Username={UserTextBox.Text.Trim()};" +
                                $"Password={PasswordBox.Password.Trim()};" + //(string)PasswordBox.Password.Trim()
                                $"Timeout=5;";

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    string debugString = conn.FullState.ToString();
                    MessageBox.Show($"Connection state: {debugString}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw;
            }
            finally 
            {
                //conn.Close();
            }

        }

        private void SaveButton_Click_SaveConnection(object sender, RoutedEventArgs e)
        {
            // save connection settings to dbconfig.xml

            string connName = ConnectionNameTextBox.Text.Trim();

            string provider = PostgreSQLRadioButton.IsChecked == true ? "Npgsql" : "SqlClient"; //MSSQL not implemented

            bool isDefault = DefaultConnectionCheckBox.IsChecked ?? false;

            string host = HostTextBox.Text.Trim();
            string port = PortTextBox.Text.Trim();
            string database = DatabaseTextBox.Text.Trim();
            string user = UserTextBox.Text.Trim();

            string password = PasswordBox.Password.Trim();

            // validate fields
            if (string.IsNullOrEmpty(connName) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Fill all the fields", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // ISSUE overflow Validate()
            //if (!Validate(HostTextBox) || !Validate(PortTextBox))
            //{
            //    MessageBox.Show("Correct errors before saving", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            MessageBox.Show("Saving connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                XDocument configXml = File.Exists(dbConfigPath) ? XDocument.Load(dbConfigPath) : new XDocument(new XElement("ConnectionSettings"));

                XElement root = configXml.Element("ConnectionSettings");

                // override default connection
                if (isDefault)
                {
                    foreach (XElement conn in root.Elements("Connection"))
                    {
                        conn.Element("IsDefault").Value = "false";
                    }
                }

                if (isEditMode && selectedConnection != null)
                {
                    XElement existingConnection = root
                        .Elements("Connection")
                        .FirstOrDefault(c => 
                            c.Element("ConnectionName")?.Value == selectedConnection.ConnectionName);
                    if (existingConnection != null) 
                    {
                        existingConnection.Element("ConnectionName").Value = connName;
                        existingConnection.Element("Provider").Value = provider;
                        existingConnection.Element("IsDefault").Value = isDefault.ToString();
                        existingConnection.Element("Host").Value = host;
                        existingConnection.Element("Port").Value = port;
                        existingConnection.Element("Database").Value = database;
                        existingConnection.Element("User").Value = user;
                        existingConnection.Element("Password").Value = password;
                    }
                }
                else
                {
                    // new connection
                    XElement newConnection = new XElement("Connection",
                            new XElement("ConnectionName", connName),
                            new XElement("Provider", provider),
                            new XElement("IsDefault", isDefault),
                            new XElement("Host", host),
                            new XElement("Port", port),
                            new XElement("Database", database),
                            new XElement("User", user),
                            new XElement("Password", password)
                        );
                    root.Add(newConnection);
                }

                configXml.Save(dbConfigPath);

                // refresh connection list
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LoadConnectionSettings();
                }

                MessageBox.Show("Parameters saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
