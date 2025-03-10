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

        // ctor
        public ConnectionWindow()
        {
            InitializeComponent();

            LoadExistingSettings();
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

                    string host = root.Element("Host")?.Value;
                    string port = root.Element("Port")?.Value;
                    string database = root.Element("Database")?.Value;
                    string user = root.Element("User")?.Value;
                    string password = root.Element("Password")?.Value;

                    //txtHost.Text = host;
                    //txtPort.Text = port;
                    //txtDatabase.Text = database;
                    //txtUser.Text = user;
                    //txtPassword.Text = password;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestConnectionButton_Click_TestConnection(object sender, RoutedEventArgs e)
        {
            //
            MessageBox.Show("Testing connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // check connection state Npgsql
        }

        private void SaveButton_Click_SaveConnection(object sender, RoutedEventArgs e)
        {
            //
            MessageBox.Show("Saving connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // save connection settings to dbconfig.xml


            // TO BE DONE

            //    string host = txtHost.Text.Trim();
            //    string port = txtPort.Text.Trim();
            //    string database = txtDatabase.Text.Trim();
            //    string user = txtUser.Text.Trim();
            //    string password = txtPassword.Password.Trim();

            //    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
            //        string.IsNullOrEmpty(database) || string.IsNullOrEmpty(user) ||
            //        string.IsNullOrEmpty(password))
            //    {
            //        MessageBox.Show("Fill all the feilds", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return;
            //    }

            //    try
            //    {
            //        XDocument configXml = new XDocument(
            //            new XElement("ConnectionSettings",
            //                new XElement("Host", host),
            //                new XElement("Port", port),
            //                new XElement("Database", database),
            //                new XElement("User", user),
            //                new XElement("Password", password)
            //            )
            //        );
            //        configXml.Save(dbConfigPath);
            //        MessageBox.Show("Parameters saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            //        this.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error saving parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
        }

        private void CancelButton_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
