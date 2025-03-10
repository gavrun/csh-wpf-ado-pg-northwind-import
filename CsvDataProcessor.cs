using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Data;

namespace csh_wpf_ado_pg_northwind_import
{
    public class CsvDataProcessor
    {
        // ETL input file CSV ","

        // ETL output array of rows

        public DataTable ProcessCsv(string filePath)
        {
            // create DataTable 
            DataTable dataTable = new DataTable();

            List<string[]> rows = new List<string[]>();

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8)) 
                {
                    bool isFirstRow = true;

                    // delimiter not ","
                    //char delimiter = DetectDelimiter(filePath);


                    while (!reader.EndOfStream)
                    {
                        // read line
                        var line = reader.ReadLine();

                        if (!string.IsNullOrWhiteSpace(line)) // skip empty lines
                        {
                            //string[] values = line.Split(','); // split by comma ","
                            // split by comma "," but ignore commas inside quotes
                            string[] values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                            //string[] values = Regex.Split(line, $"{delimiter}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                            if (isFirstRow)
                            {
                                foreach (var header in values)
                                {
                                    dataTable.Columns.Add(header.Trim('"')); // clean headers
                                }
                                isFirstRow = false;
                            }
                            else
                            {
                                rows.Add(values);
                            }
                        }
                    }
                }

                // fill DataTable 
                foreach (var row in rows)
                {
                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return dataTable;
        }

        private char DetectDelimiter(string filePath)
        {
            // read first line
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string firstLine = reader.ReadLine();

                if (firstLine.Contains(";")) 
                    return ';';

                return ','; // default ","
            }
        }
    }
}
