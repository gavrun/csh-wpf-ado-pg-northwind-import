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

            LoadCustomers();
            LoadProducts();

            OrderDatePicker.SelectedDate = DateTime.Today;
        }

        // populate dropdowns with data 
        private void LoadCustomers()
        {
            string query = "SELECT customer_id, company_name FROM customers;";
            var customers = QueryHelper.ExecuteQuery(query, reader => new
            {
                CustomerId = reader.GetString(0),
                CompanyName = reader.GetString(1)
            });

            CustomerComboBox.ItemsSource = customers;
            CustomerComboBox.DisplayMemberPath = "CompanyName";
            CustomerComboBox.SelectedValuePath = "CustomerId";
        }

        private void LoadProducts()
        {
            string query = "SELECT product_id, product_name FROM products;";
            var products = QueryHelper.ExecuteQuery(query, reader => new
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1)
            });

            ProductComboBox.ItemsSource = products;
            ProductComboBox.DisplayMemberPath = "ProductName";
            ProductComboBox.SelectedValuePath = "ProductId";
        }


        private void SaveButton_Click_SaveOrder(object sender, RoutedEventArgs e)
        {
            string customerId = CustomerComboBox.SelectedValue?.ToString() ?? "-1";

            int productId = ProductComboBox.SelectedValue != null ? Convert.ToInt16(ProductComboBox.SelectedValue) : -1;

            int productQuantity;
            bool isValidQuantity = int.TryParse(QuantityTextBox.Text, out productQuantity);

            // default date today
            DateTime orderDate = OrderDatePicker.SelectedDate ?? DateTime.Today;

            if (CustomerComboBox.SelectedItem == null || ProductComboBox.SelectedItem == null || !isValidQuantity || productQuantity <= 0)
            {
                MessageBox.Show("Please enter valid data. e.g. Quantity \"1\"", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // DEBUG
            //MessageBox.Show($"Order saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // upload data to database
            // SQL query: INSERT INTO orders (customer, product, quantity, order_date) VALUES ('customer', 'product', quantity, 'order_date');
            // SQL query: INSERT INTO order_details (order, product, quantity, order_date) VALUES ('order', 'product', quantity, 'order_date');

            try
            {
                // WORKAROUND
                // get unit_price
                string priceQuery = "SELECT unit_price FROM products WHERE product_id = @productId";
                var priceResult = QueryHelper.ExecuteScalar(priceQuery, new Dictionary<string, object> 
                { 
                    { "@productId", productId } 
                });
                // set discount 
                decimal unitPrice = Convert.ToDecimal(priceResult);
                decimal discount = 0.0m;

                List<string> queries = new List<string>
                {
                    "INSERT INTO orders (order_id, customer_id, order_date) " +
                        "VALUES ((SELECT COALESCE(MAX(order_id), 0) + 1 FROM orders), @customer, @orderDate) " +
                        "RETURNING order_id",
                    "INSERT INTO order_details (order_id, product_id, unit_price, quantity, discount) " +
                        "VALUES (@orderId, @product, @unitPrice, @quantity, @discount)"
                };

                var parametersList = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "@customer", customerId },
                        { "@orderDate", orderDate }
                    },
                    new Dictionary<string, object>
                    {
                        { "@orderId", 0 }, // pending
                        { "@product", productId },
                        { "@unitPrice", unitPrice },
                        { "@quantity", productQuantity },
                        { "@discount", discount }
                    }
                };

                // get order_id
                int orderId = Convert.ToInt16(QueryHelper.ExecuteScalar(queries[0], parametersList[0]));
                parametersList[1]["@orderId"] = orderId;
                
                // put order_details
                QueryHelper.ExecuteNonQuery(queries[1], parametersList[1]);

                MessageBox.Show("Order created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
