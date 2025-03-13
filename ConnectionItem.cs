using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csh_wpf_ado_pg_northwind_import
{
    public class ConnectionItem
    {
        // connection item object in dbconfig.xml
        public string ConnectionName { get; set; }
        public string Provider { get; set; }
        public bool IsDefault { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        // ctor unticipating
        public ConnectionItem()
        {
            ConnectionName = "";
            Provider = "";
            IsDefault = false;
            Host = "";
            Port = "";
            Database = "";
            User = "";
            Password = "";
        }

        public ConnectionItem(string connectionName, string provider, bool isDefault, string host, string port, string database, string user, string password)
        {
            ConnectionName = connectionName;
            Provider = provider;
            IsDefault = isDefault;
            Host = host;
            Port = port;
            Database = database;
            User = user;
            Password = password;
        }

        //
        public string GetConnectionString()
        {
            return $"Host={Host};Port={Port};Database={Database};Username={User};Password={Password};Timeout=5;"; //Pooling=true;
        }

    }
}
