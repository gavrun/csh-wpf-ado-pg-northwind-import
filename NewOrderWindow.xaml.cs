using System;
using System.Collections.Generic;
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

namespace csh_wpf_ado_pg_northwind_import
{
    /// <summary>
    /// Interaction logic for NewOrderWindow.xaml
    /// </summary>
    public partial class NewOrderWindow : Window
    {
        public NewOrderWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click_SaveOrder(object sender, RoutedEventArgs e)
        {
            string client = ClientTextBox.Text;
            string product = ProductTextBox.Text;

            int productQuantity;

            bool isValidQuantity = int.TryParse(QuantityTextBox.Text, out productQuantity);

            // default date today
            DateTime orderDate = OrderDatePicker.SelectedDate ?? DateTime.Today;

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(product) || !isValidQuantity || productQuantity < 0)
            {
                MessageBox.Show($"Input order details", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //MessageBox.Show($"Order saved\nClient: {client}\nProduct: {product}\nQuantity: {productQuantity}\nDate: {orderDate.ToShortDateString()}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            MessageBox.Show($"Order saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // upload data to database
            // SQL query: INSERT INTO orders (client, product, quantity, order_date) VALUES ('client', 'product', quantity, 'order_date');
        }

        private void CancelButton_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
