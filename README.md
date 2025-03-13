# csh-wpf-ado-pg-northwind-import

## Overview

It is a WPF application for working with the Northwind database, developed using C#, WPF, and ADO.NET for connecting to PostgreSQL.

Northwind is a training (demo) database, originally developed by Microsoft as an example of a business-oriented relational database. It contains sales data of a fictitious trading company Northwind Traders, which is engaged in the export and import of food products worldwide. Data source: https://github.com/pthom/northwind_psql 

## Requirements and steps to deploy

* DevEnv: Visual Studio Community 2022 (17.13.2)
* .NET: 8.0 (LTS)
* DBMS: PostgreSQL 17
* Tools: pgAdmin4 9.0
* Libraries: Npgsql

### 1. PostgreSQL
* Download and install PostgreSQL 17.
* Ensure pgAdmin4 is installed for database management.

### 2. Database 
1. Open pgAdmin4
2. Connect to the PostgreSQL server
3. Open Query Tool and execute the command:
```sql
CREATE DATABASE northwind TEMPLATE template0;
```

### 3. Data Import
1. Open Query Tool in pgAdmin4 connected to `northwind` database
2. Execute the `/northwind_psql/northwind.sql` script
3. Verify that schema and data imported

### 4. Building 
1. Install Visual Studio 2022
2. Add the .NET Desktop Development workload
3. Open VS and select Menu\Git\Clone Repository
4. Clone repository over HTTP/SSH
5. Restore dependencies
6. Build solution
7. Start Without Debugging (Ctrl+F5)

### 5. Connection
1. The application uses `dbconfig.xml` cofiguration file to store PostgreSQL connection settings.
2. The application creates and stores connection settings in a format `/dbconfig/dbconfig.xml`.
3. Next time the application starts, it will reuse the stored settings.

### 6. Features

* Connect to PostgreSQL database and multible connections.
* Display lists of products, orders, and customers.
* Filter data, search filter, entry details.
* Create new orders, simplified form.
* Import products data from CSV files, example `/etldata/products_data.csv`.
* Manage connections in UI, connecttion status, reconnect.
* Classic UI style, low focus on design and adaptiveness.
* Box notifications for demostrating logic and debug commented out.
* Toast UI notification popups with native SQL statements that exectued towards the database.
* Lots of TBD :)