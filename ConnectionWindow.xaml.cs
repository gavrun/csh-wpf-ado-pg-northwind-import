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

        // ctor
        public ConnectionWindow(bool isEdit = false)
        {
            InitializeComponent();
            isEditMode = isEdit;

            if (isEditMode)
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
                if (File.Exists(dbConfigPath))
                {
                    XDocument configXml = XDocument.Load(dbConfigPath);

                    XElement root = configXml.Element("ConnectionSettings");

                    string connName = root.Element("ConnectionName")?.Value;
                    bool isDefault = bool.TryParse(root.Element("DefaultConnectionCheckBox")?.Value, out bool result);

                    string host = root.Element("Host")?.Value;
                    string port = root.Element("Port")?.Value;
                    string database = root.Element("Database")?.Value;
                    string user = root.Element("User")?.Value;
                    string password = root.Element("Password")?.Value;

                    ConnectionNameTextBox.Text = connName;
                    HostTextBox.Text = host;
                    PortTextBox.Text = port;
                    DatabaseTextBox.Text = database;
                    UserTextBox.Text = user;

                    PasswordBox.Password = password;
                    
                }
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
        }

        private void SaveButton_Click_SaveConnection(object sender, RoutedEventArgs e)
        {
            // save connection settings to dbconfig.xml

            string connName = ConnectionNameTextBox.Text.Trim();
            bool isDefault = DefaultConnectionCheckBox.IsChecked ?? false;

            string host = HostTextBox.Text.Trim();
            string port = PortTextBox.Text.Trim();
            string database = DatabaseTextBox.Text.Trim();
            string user = UserTextBox.Text.Trim();

            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
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

            //
            MessageBox.Show("Saving connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                XDocument configXml = new XDocument(
                    new XElement("ConnectionSettings",
                        new XElement("ConnectionName", connName),
                        new XElement("DefaultConnectionCheckBox", isDefault),
                        new XElement("Host", host),
                        new XElement("Port", port),
                        new XElement("Database", database),
                        new XElement("User", user),
                        new XElement("Password", password)
                    )
                );
                configXml.Save(dbConfigPath);

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
